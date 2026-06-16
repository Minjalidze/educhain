import type { VerificationStatus } from "@/types/api";
import type { ReactNode } from "react";

type BadgeTone = "green" | "red" | "amber" | "blue" | "gray";

export function Badge({ children, tone = "gray" }: { children: ReactNode; tone?: BadgeTone }) {
  return <span className={`badge badge-${tone}`}>{children}</span>;
}

export function DocumentStatusBadge({ revoked }: { revoked: boolean }) {
  return revoked ? <Badge tone="red">Отозван</Badge> : <Badge tone="green">Действителен</Badge>;
}

export function VerificationBadge({ status }: { status: VerificationStatus }) {
  if (status === "Valid") {
    return <Badge tone="green">Действителен</Badge>;
  }

  if (status === "Revoked") {
    return <Badge tone="red">Отозван</Badge>;
  }

  return <Badge tone="amber">Не найден</Badge>;
}
