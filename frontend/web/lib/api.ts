import { getToken } from "@/lib/auth";
import type {
  AddDocumentResult,
  ApiProblem,
  CreateUserRequest,
  DashboardStats,
  DocumentDto,
  LoginResponse,
  UpdateUserRequest,
  UserDto,
  VerifyDocumentResult
} from "@/types/api";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "";

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);

  if (!(init.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const token = getToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${apiUrl}${path}`, {
    ...init,
    headers
  });

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ApiProblem;
    throw new Error(problem.detail || problem.title || "Запрос завершился ошибкой.");
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export function login(email: string, password: string) {
  return request<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password })
  });
}

export function getDashboard() {
  return request<DashboardStats>("/api/documents/dashboard");
}

export function listDocuments() {
  return request<DocumentDto[]>("/api/documents");
}

export function addDocument(formData: FormData) {
  return request<AddDocumentResult>("/api/documents", {
    method: "POST",
    body: formData
  });
}

export function verifyDocument(formData: FormData) {
  return request<VerifyDocumentResult>("/api/documents/verify", {
    method: "POST",
    body: formData
  });
}

export function revokeDocument(id: string) {
  return request<DocumentDto>(`/api/documents/${id}/revoke`, {
    method: "POST"
  });
}

export function listUsers() {
  return request<UserDto[]>("/api/users");
}

export function createUser(payload: CreateUserRequest) {
  return request<UserDto>("/api/users", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

export function updateUser(id: string, payload: UpdateUserRequest) {
  return request<UserDto>(`/api/users/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload)
  });
}

export function deleteUser(id: string) {
  return request<void>(`/api/users/${id}`, {
    method: "DELETE"
  });
}
