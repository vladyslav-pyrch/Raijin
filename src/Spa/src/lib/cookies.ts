// ─── Cookie helpers ───────────────────────────────────────────────────────────

const MAX_AGE = 60 * 60 * 24 * 365; // 1 year

export function getCookie(name: string): string | undefined {
  return document.cookie
    .split('; ')
    .find((row) => row.startsWith(name + '='))
    ?.split('=')[1];
}

export function setCookie(name: string, value: string): void {
  document.cookie = `${name}=${value}; path=/; max-age=${MAX_AGE}; SameSite=Lax`;
}
