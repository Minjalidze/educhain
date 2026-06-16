"use client";

import { clearSession, getUser } from "@/lib/auth";
import type { AuthUser } from "@/types/api";
import { FileCheck2, LayoutDashboard, LogOut, PlusCircle, SearchCheck, Settings, ShieldCheck } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import type { ReactNode } from "react";

const publicLinks = [{ href: "/verify", label: "Проверка", icon: SearchCheck }];

const privateLinks = [
  { href: "/dashboard", label: "Сводка", icon: LayoutDashboard },
  { href: "/documents", label: "Документы", icon: FileCheck2 },
  { href: "/documents/new", label: "Добавить", icon: PlusCircle }
];

const adminLinks = [{ href: "/admin", label: "Админ", icon: Settings }];

export function AppShell({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const pathname = usePathname();
  const router = useRouter();

  useEffect(() => {
    setUser(getUser());
  }, [pathname]);

  function logout() {
    clearSession();
    setUser(null);
    router.push("/");
  }

  const links = user
    ? [
        ...publicLinks,
        ...(user.role === "Verifier" ? [] : privateLinks),
        ...(user.role === "Admin" ? adminLinks : [])
      ]
    : publicLinks;

  return (
    <>
      <header className="topbar">
        <Link href="/" className="brand" aria-label="Главная">
          <ShieldCheck size={24} />
          <span>EduChain Verify</span>
        </Link>
        <nav className="nav" aria-label="Основное меню">
          {links.map((item) => {
            const Icon = item.icon;
            return (
              <Link key={item.href} className={pathname === item.href ? "nav-link active" : "nav-link"} href={item.href}>
                <Icon size={18} />
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>
        <div className="session" aria-live="polite">
          {user && (
            <>
              <span className="session-user" title={`${user.fullName} · ${user.role}`}>
                {user.fullName}
              </span>
              <button className="icon-button" onClick={logout} title="Выйти" aria-label="Выйти" type="button">
                <LogOut size={18} />
              </button>
            </>
          )}
        </div>
      </header>
      <main className="page">{children}</main>
    </>
  );
}
