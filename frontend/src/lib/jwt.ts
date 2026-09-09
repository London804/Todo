// Minimal JWT payload decoding for the browser.
//
// IMPORTANT: this reads the token's claims for DISPLAY ONLY (e.g. the user's
// email). A JWT payload is just base64url-encoded JSON — it is NOT encrypted and
// NOT verified here. Never trust these values for security decisions; the server
// validates the signature on every request. The client only peeks.

export interface JwtPayload {
  sub?: string
  email?: string
  exp?: number // expiry, as seconds since the Unix epoch
  [claim: string]: unknown
}

function base64UrlDecode(input: string): string {
  // JWT uses base64url ('-' and '_'); convert to standard base64 and pad.
  let base64 = input.replace(/-/g, '+').replace(/_/g, '/')
  const padding = base64.length % 4
  if (padding) base64 += '='.repeat(4 - padding)
  return atob(base64)
}

export function decodeJwt(token: string): JwtPayload | null {
  try {
    const payload = token.split('.')[1] // header.PAYLOAD.signature
    return JSON.parse(base64UrlDecode(payload)) as JwtPayload
  } catch {
    return null // malformed token
  }
}
