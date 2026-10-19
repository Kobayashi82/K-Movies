-- Una nota de 0 estrellas es "sin nota" (el usuario no quiere el 0 como nota).

UPDATE history SET rating = NULL WHERE rating <= 0;
