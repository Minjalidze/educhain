"use client";

import type { AuthUser, LoginResponse } from "@/types/api";

const tokenKey = "document-verifier-token";
const userKey = "document-verifier-user";

export function saveSession(response: LoginResponse) {
  localStorage.setItem(tokenKey, response.token);
  localStorage.setItem(userKey, JSON.stringify(response.user));
}

export function getToken() {
  return typeof window === "undefined" ? null : localStorage.getItem(tokenKey);
}

export function getUser(): AuthUser | null {
  if (typeof window === "undefined") {
    return null;
  }

  const raw = localStorage.getItem(userKey);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    clearSession();
    return null;
  }
}

export function clearSession() {
  localStorage.removeItem(tokenKey);
  localStorage.removeItem(userKey);
}
