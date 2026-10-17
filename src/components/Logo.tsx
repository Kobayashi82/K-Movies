/** Logo: un fotograma con una "K". */
export function Logo({ size = 24 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 32 32" aria-hidden="true" className="logo">
      <rect x="2" y="5" width="28" height="22" rx="5" fill="var(--accent)" />
      <g fill="var(--accent-ink)" opacity="0.55">
        <rect x="4.5" y="8" width="2.5" height="2.5" rx="0.6" />
        <rect x="4.5" y="14.75" width="2.5" height="2.5" rx="0.6" />
        <rect x="4.5" y="21.5" width="2.5" height="2.5" rx="0.6" />
        <rect x="25" y="8" width="2.5" height="2.5" rx="0.6" />
        <rect x="25" y="14.75" width="2.5" height="2.5" rx="0.6" />
        <rect x="25" y="21.5" width="2.5" height="2.5" rx="0.6" />
      </g>
      <path d="M11.5 9.5v13M11.5 16.5l7-7M14 14l5.5 8.5" stroke="var(--accent-ink)" strokeWidth="2.6" strokeLinecap="round" strokeLinejoin="round" fill="none" />
    </svg>
  );
}
