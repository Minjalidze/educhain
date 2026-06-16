"use client";

import { getToken, getUser } from "@/lib/auth";
import { StateMessage } from "@/components/ui/StateMessage";
import type { UserRole } from "@/types/api";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import type { ReactNode } from "react";

export function RequireAuth({ children, roles }: { children: ReactNode; roles?: UserRole[] }) {
  const pathname = usePathname();
  const router = useRouter();
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    const user = getUser();
    const hasSession = Boolean(getToken() && user);

    if (!hasSession) {
      const returnTo = pathname && pathname !== "/login" ? `?returnTo=${encodeURIComponent(pathname)}` : "";
      router.replace(`/login${returnTo}`);
      setAllowed(false);
      return;
    }

    if (roles && user && !roles.includes(user.role)) {
      router.replace("/dashboard");
      setAllowed(false);
      return;
    }

    setAllowed(true);
  }, [pathname, roles, router]);

  if (!allowed) {
    return (
      <section className="auth-guard">
        <StateMessage type="info">Проверяем доступ...</StateMessage>
      </section>
    );
  }

  return <>{children}</>;
}
