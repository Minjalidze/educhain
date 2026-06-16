"use client";

import { getToken, getUser } from "@/lib/auth";
import { LayoutDashboard, LockKeyhole, SearchCheck } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";

export function HomeActions() {
  const [authenticated, setAuthenticated] = useState(false);

  useEffect(() => {
    setAuthenticated(Boolean(getToken() && getUser()));
  }, []);

  return (
    <div className="hero-actions">
      <Link className="button button-primary" href="/verify">
        <SearchCheck size={18} />
        Проверить документ
      </Link>
      {authenticated ? (
        <Link className="button button-secondary" href="/dashboard">
          <LayoutDashboard size={18} />
          Открыть сводку
        </Link>
      ) : (
        <Link className="button button-secondary" href="/login">
          <LockKeyhole size={18} />
          Войти в систему
        </Link>
      )}
    </div>
  );
}
