"use client";

import { RequireAuth } from "@/components/auth/RequireAuth";
import { StateMessage } from "@/components/ui/StateMessage";
import { createUser, deleteUser, listUsers, updateUser } from "@/lib/api";
import { getUser } from "@/lib/auth";
import type { UserDto, UserRole } from "@/types/api";
import { FileCheck2, Pencil, PlusCircle, RefreshCw, ShieldCheck, Trash2, UsersRound } from "lucide-react";
import Link from "next/link";
import { FormEvent, useEffect, useMemo, useState } from "react";

const adminRoles: UserRole[] = ["Admin"];
const userRoles: UserRole[] = ["Admin", "Issuer", "Verifier"];

const roleLabels: Record<UserRole, string> = {
  Admin: "Администратор",
  Issuer: "Организация",
  Verifier: "Проверяющий"
};

const emptyForm = {
  email: "",
  fullName: "",
  password: "",
  role: "Issuer" as UserRole
};

export default function AdminPage() {
  return (
    <RequireAuth roles={adminRoles}>
      <AdminContent />
    </RequireAuth>
  );
}

function AdminContent() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const currentUser = useMemo(() => getUser(), []);
  const editing = users.find((user) => user.id === editingId) ?? null;

  async function load() {
    setLoading(true);
    setError(null);

    try {
      setUsers(await listUsers());
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Не удалось загрузить пользователей.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  function startEdit(user: UserDto) {
    setEditingId(user.id);
    setForm({
      email: user.email,
      fullName: user.fullName,
      password: "",
      role: user.role
    });
    setError(null);
    setMessage(null);
  }

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setMessage(null);

    try {
      if (editingId) {
        await updateUser(editingId, {
          email: form.email,
          fullName: form.fullName,
          role: form.role,
          password: form.password.trim() ? form.password : undefined
        });
        setMessage("Пользователь обновлён.");
      } else {
        await createUser(form);
        setMessage("Пользователь создан.");
      }

      resetForm();
      await load();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Не удалось сохранить пользователя.");
    } finally {
      setSaving(false);
    }
  }

  async function removeUser(user: UserDto) {
    if (!confirm(`Удалить пользователя ${user.fullName}?`)) {
      return;
    }

    setError(null);
    setMessage(null);

    try {
      await deleteUser(user.id);
      setMessage("Пользователь удалён.");
      await load();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Не удалось удалить пользователя.");
    }
  }

  return (
    <section className="stack admin-page">
      <div className="section-heading">
        <UsersRound size={28} />
        <div>
          <h1>Админ-панель</h1>
          <p>Управление пользователями, ролями и доступом к разделам системы.</p>
        </div>
      </div>

      <div className="admin-summary">
        <div className="card stat">
          <UsersRound size={22} />
          <span>Пользователи</span>
          <strong>{users.length}</strong>
        </div>
        <Link className="card admin-link-card" href="/documents">
          <FileCheck2 size={22} />
          <span>Все документы</span>
          <strong>Открыть реестр</strong>
        </Link>
        <Link className="card admin-link-card" href="/dashboard">
          <ShieldCheck size={22} />
          <span>Контроль системы</span>
          <strong>Открыть сводку</strong>
        </Link>
      </div>

      {(error || message) && (
        <div className="admin-messages">
          {error && <StateMessage type="error">{error}</StateMessage>}
          {message && <StateMessage type="success">{message}</StateMessage>}
        </div>
      )}

      <div className="admin-grid">
        <form className="card form-card admin-form" onSubmit={onSubmit}>
          <div className="panel-title">
            <h2>{editing ? "Редактирование пользователя" : "Новый пользователь"}</h2>
            {editing && (
              <button className="button button-secondary button-compact" onClick={resetForm} type="button">
                Отмена
              </button>
            )}
          </div>

          <label>
            Имя
            <input
              value={form.fullName}
              onChange={(event) => setForm((value) => ({ ...value, fullName: event.target.value }))}
              required
              placeholder="Название организации или ФИО"
            />
          </label>
          <label>
            Email
            <input
              value={form.email}
              onChange={(event) => setForm((value) => ({ ...value, email: event.target.value }))}
              required
              type="email"
              placeholder="user@example.local"
            />
          </label>
          <label>
            Роль
            <select
              value={form.role}
              onChange={(event) => setForm((value) => ({ ...value, role: event.target.value as UserRole }))}
            >
              {userRoles.map((role) => (
                <option key={role} value={role}>
                  {roleLabels[role]}
                </option>
              ))}
            </select>
          </label>
          <label>
            Пароль
            <input
              value={form.password}
              onChange={(event) => setForm((value) => ({ ...value, password: event.target.value }))}
              required={!editing}
              type="password"
              placeholder={editing ? "Оставьте пустым, чтобы не менять" : "Минимум 6 символов"}
            />
          </label>

          <button className="button button-primary" disabled={saving} type="submit">
            <PlusCircle size={18} />
            {saving ? "Сохранение..." : editing ? "Сохранить изменения" : "Создать пользователя"}
          </button>
        </form>

        <div className="card table-card admin-users-card">
          <div className="panel-title">
            <h2>Пользователи</h2>
            <button className="icon-button" onClick={load} title="Обновить" type="button">
              <RefreshCw size={17} />
            </button>
          </div>

          {loading ? (
            <StateMessage type="info">Загрузка пользователей...</StateMessage>
          ) : (
            <div className="responsive-list">
              {users.map((user) => (
                <div className={editingId === user.id ? "user-row active" : "user-row"} key={user.id}>
                  <div className="user-main">
                    <strong>{user.fullName}</strong>
                    <span>{user.email}</span>
                  </div>
                  <span className={`role-pill role-${user.role.toLowerCase()}`}>{roleLabels[user.role]}</span>
                  <div className="row-actions">
                    <button className="icon-button" onClick={() => startEdit(user)} title="Редактировать" type="button">
                      <Pencil size={16} />
                    </button>
                    <button
                      className="icon-button danger"
                      disabled={user.id === currentUser?.id}
                      onClick={() => removeUser(user)}
                      title={user.id === currentUser?.id ? "Нельзя удалить текущую учётную запись" : "Удалить"}
                      type="button"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
