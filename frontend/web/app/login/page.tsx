"use client";

import { StateMessage } from "@/components/ui/StateMessage";
import { login } from "@/lib/api";
import { getToken, getUser, saveSession } from "@/lib/auth";
import { KeyRound, LogIn } from "lucide-react";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";

function getSafeReturnTo(value: string | null) {
  return value && value.startsWith("/") && !value.startsWith("//") ? value : "/dashboard";
}

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("issuer@example.local");
  const [password, setPassword] = useState("issuer123");
  const [ready, setReady] = useState(false);
  const [returnTo, setReturnTo] = useState("/dashboard");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (getToken() && getUser()) {
      router.replace("/dashboard");
      return;
    }

    setReturnTo(getSafeReturnTo(new URLSearchParams(window.location.search).get("returnTo")));
    setReady(true);
  }, [router]);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await login(email, password);
      saveSession(response);
      router.push(returnTo);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Не удалось войти.");
    } finally {
      setLoading(false);
    }
  }

  if (!ready) {
    return (
      <section className="form-layout auth-layout">
        <StateMessage type="info">Проверяем сессию...</StateMessage>
      </section>
    );
  }

  return (
    <section className="form-layout auth-layout">
      <div className="section-heading">
        <KeyRound size={28} />
        <div>
          <h1>Вход</h1>
          <p>Авторизация Issuer или Admin для регистрации и отзыва документов.</p>
        </div>
      </div>

      <form className="card form-card" onSubmit={onSubmit}>
        <label>
          Email
          <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
        </label>
        <label>
          Пароль
          <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
        </label>

        {error && <StateMessage type="error">{error}</StateMessage>}

        <button className="button button-primary" disabled={loading} type="submit">
          <LogIn size={18} />
          {loading ? "Вход..." : "Войти"}
        </button>
      </form>
    </section>
  );
}
