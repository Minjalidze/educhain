import type { ReactNode } from "react";

export function StateMessage({
  type,
  children
}: {
  type: "success" | "error" | "info";
  children: ReactNode;
}) {
  return <div className={`state state-${type}`}>{children}</div>;
}
