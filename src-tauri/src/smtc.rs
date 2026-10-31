//! Controles multimedia de Windows (SMTC): el panel de "lo que suena" (título, póster, barra de
//! progreso) y las teclas multimedia del teclado o los auriculares, aunque el reproductor no tenga
//! el foco. Cada ventana del reproductor es una sesión; Windows manda las teclas a la última activa.

use anyhow::Result;
use std::path::Path;
use windows::Foundation::{TimeSpan, TypedEventHandler};
use windows::Media::{
    MediaPlaybackStatus, MediaPlaybackType, PlaybackPositionChangeRequestedEventArgs, SystemMediaTransportControls,
    SystemMediaTransportControlsButton, SystemMediaTransportControlsButtonPressedEventArgs,
    SystemMediaTransportControlsTimelineProperties,
};
use windows::Storage::StorageFile;
use windows::Storage::Streams::RandomAccessStreamReference;
use windows::Win32::Foundation::HWND;
use windows::Win32::System::WinRT::ISystemMediaTransportControlsInterop;
use windows::core::{HSTRING, factory};

/// Lo que se pide desde Windows.
pub enum Action {
    Play,
    Pause,
    /// Salto relativo, en segundos.
    Seek(f64),
    /// Ir a ese segundo (al arrastrar la barra del panel).
    SeekTo(f64),
}

/// Lo que se ve en el panel de Windows.
pub struct Info<'a> {
    pub title: &'a str,
    pub subtitle: Option<String>,
    pub cover: Option<&'a Path>,
}

pub struct Controls {
    smtc: SystemMediaTransportControls,
    button_token: i64,
    seek_token: i64,
}

/// Siguiente y Anterior saltan 30 s; Avance y Retroceso rápido, 10 s.
const SKIP: f64 = 30.0;
const STEP: f64 = 10.0;

fn span(seconds: f64) -> TimeSpan {
    TimeSpan { Duration: (seconds.max(0.0) * 10_000_000.0) as i64 }
}

impl Controls {
    pub fn new(hwnd: isize, info: &Info, on_action: impl Fn(Action) + Send + Sync + 'static) -> Result<Self> {
        let interop = factory::<SystemMediaTransportControls, ISystemMediaTransportControlsInterop>()?;
        // SAFETY: `hwnd` es la ventana del reproductor, viva mientras dura la sesión.
        let smtc: SystemMediaTransportControls = unsafe { interop.GetForWindow(HWND(hwnd as _))? };
        smtc.SetIsEnabled(true)?;
        smtc.SetIsPlayEnabled(true)?;
        smtc.SetIsPauseEnabled(true)?;
        smtc.SetIsNextEnabled(true)?;
        smtc.SetIsPreviousEnabled(true)?;
        smtc.SetIsFastForwardEnabled(true)?;
        smtc.SetIsRewindEnabled(true)?;
        smtc.SetPlaybackStatus(MediaPlaybackStatus::Changing)?;

        let updater = smtc.DisplayUpdater()?;
        updater.SetType(MediaPlaybackType::Video)?;
        let video = updater.VideoProperties()?;
        video.SetTitle(&HSTRING::from(info.title))?;
        if let Some(sub) = &info.subtitle {
            video.SetSubtitle(&HSTRING::from(sub.as_str()))?;
        }
        // Sin póster sale el icono de la app.
        if let Some(path) = info.cover
            && let Ok(file) = StorageFile::GetFileFromPathAsync(&HSTRING::from(path)).and_then(|op| op.join())
        {
            updater.SetThumbnail(&RandomAccessStreamReference::CreateFromFile(&file)?)?;
        }
        updater.Update()?;

        let on_action = std::sync::Arc::new(on_action);
        let act = on_action.clone();
        let button_token = smtc.ButtonPressed(&TypedEventHandler::<
            SystemMediaTransportControls,
            SystemMediaTransportControlsButtonPressedEventArgs,
        >::new(move |_, args| {
            let action = match args.ok()?.Button()? {
                SystemMediaTransportControlsButton::Play => Action::Play,
                SystemMediaTransportControlsButton::Pause => Action::Pause,
                SystemMediaTransportControlsButton::Next => Action::Seek(SKIP),
                SystemMediaTransportControlsButton::Previous => Action::Seek(-SKIP),
                SystemMediaTransportControlsButton::FastForward => Action::Seek(STEP),
                SystemMediaTransportControlsButton::Rewind => Action::Seek(-STEP),
                _ => return Ok(()),
            };
            act(action);
            Ok(())
        }))?;
        let seek_token = smtc.PlaybackPositionChangeRequested(&TypedEventHandler::<
            SystemMediaTransportControls,
            PlaybackPositionChangeRequestedEventArgs,
        >::new(move |_, args| {
            let to = args.ok()?.RequestedPlaybackPosition()?;
            on_action(Action::SeekTo(to.Duration as f64 / 10_000_000.0));
            Ok(())
        }))?;
        Ok(Controls { smtc, button_token, seek_token })
    }

    pub fn set_paused(&self, paused: bool) {
        let status = if paused { MediaPlaybackStatus::Paused } else { MediaPlaybackStatus::Playing };
        let _ = self.smtc.SetPlaybackStatus(status);
    }

    /// Posición y duración para la barra de progreso del panel.
    pub fn set_timeline(&self, position: f64, duration: f64) {
        let set = || -> windows::core::Result<()> {
            let t = SystemMediaTransportControlsTimelineProperties::new()?;
            t.SetStartTime(span(0.0))?;
            t.SetMinSeekTime(span(0.0))?;
            t.SetEndTime(span(duration))?;
            t.SetMaxSeekTime(span(duration))?;
            t.SetPosition(span(position.min(duration)))?;
            self.smtc.UpdateTimelineProperties(&t)
        };
        let _ = set();
    }
}

impl Drop for Controls {
    fn drop(&mut self) {
        let _ = self.smtc.RemoveButtonPressed(self.button_token);
        let _ = self.smtc.RemovePlaybackPositionChangeRequested(self.seek_token);
        let _ = self.smtc.SetPlaybackStatus(MediaPlaybackStatus::Closed);
        if let Ok(u) = self.smtc.DisplayUpdater() {
            let _ = u.ClearAll();
            let _ = u.Update();
        }
        let _ = self.smtc.SetIsEnabled(false);
    }
}
