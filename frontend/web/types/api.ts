export type UserRole = "Admin" | "Issuer" | "Verifier";
export type VerificationStatus = "Valid" | "Revoked" | "NotFound";

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
}

export interface UserDto extends AuthUser {
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  email: string;
  fullName: string;
  role: UserRole;
  password?: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

export interface DocumentDto {
  id: string;
  title: string;
  documentNumber: string;
  documentType: string;
  documentHash: string;
  issuerName: string;
  issueDate: string;
  blockchainTransactionHash: string;
  contractAddress: string;
  blockchainNetwork: string;
  isRevoked: boolean;
  revokedAt: string | null;
  createdAt: string;
}

export interface AddDocumentResult {
  document: DocumentDto;
  documentHash: string;
  transactionHash: string;
  contractAddress: string;
  blockchainNetwork: string;
}

export interface VerifyDocumentResult {
  status: VerificationStatus;
  message: string;
  documentHash: string;
  blockchainStatus: string;
  document: DocumentDto | null;
}

export interface DashboardStats {
  totalDocuments: number;
  revokedDocuments: number;
  activeDocuments: number;
  verificationChecks: number;
  recentDocuments: DocumentDto[];
}

export interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
}
