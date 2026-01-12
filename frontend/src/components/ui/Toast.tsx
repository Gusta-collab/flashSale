'use client';

import { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';

export interface ToastProps {
    id: string;
    type: 'success' | 'error' | 'info';
    title: string;
    message?: string;
    duration?: number;
}

interface ToastContainerProps {
    toasts: ToastProps[];
    onRemove: (id: string) => void;
}

export function ToastContainer({ toasts, onRemove }: ToastContainerProps) {
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    if (!mounted) return null;

    return createPortal(
        <div className="fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-md">
            {toasts.map((toast) => (
                <Toast key={toast.id} {...toast} onRemove={onRemove} />
            ))}
        </div>,
        document.body
    );
}

function Toast({ id, type, title, message, duration = 5000, onRemove }: ToastProps & { onRemove: (id: string) => void }) {
    useEffect(() => {
        const timer = setTimeout(() => {
            onRemove(id);
        }, duration);

        return () => clearTimeout(timer);
    }, [id, duration, onRemove]);

    const icons = {
        success: '✅',
        error: '❌',
        info: '💡',
    };

    const colors = {
        success: 'from-green-500/20 to-green-600/10 border-green-500/50',
        error: 'from-red-500/20 to-red-600/10 border-red-500/50',
        info: 'from-blue-500/20 to-blue-600/10 border-blue-500/50',
    };

    return (
        <div
            className={`p-4 rounded-xl bg-gradient-to-r ${colors[type]} border backdrop-blur-sm
        animate-in slide-in-from-right-5 fade-in duration-300`}
            role="alert"
        >
            <div className="flex items-start gap-3">
                <span className="text-xl">{icons[type]}</span>
                <div className="flex-1">
                    <p className="font-semibold text-white">{title}</p>
                    {message && <p className="text-sm text-slate-300 mt-1">{message}</p>}
                </div>
                <button
                    onClick={() => onRemove(id)}
                    className="text-slate-400 hover:text-white transition-colors"
                >
                    ✕
                </button>
            </div>
        </div>
    );
}

// Hook para gerenciar toasts
export function useToast() {
    const [toasts, setToasts] = useState<ToastProps[]>([]);

    const addToast = (toast: Omit<ToastProps, 'id'>) => {
        const id = Math.random().toString(36).substring(7);
        setToasts((prev) => [...prev, { ...toast, id }]);
    };

    const removeToast = (id: string) => {
        setToasts((prev) => prev.filter((t) => t.id !== id));
    };

    return { toasts, addToast, removeToast };
}
