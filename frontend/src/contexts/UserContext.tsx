'use client';

import { createContext, useContext, useEffect, useState, ReactNode } from 'react';

// ════════════════════════════════════════════════════════════════════════
// User Context - Gerencia identidade do usuário (demo)
// ════════════════════════════════════════════════════════════════════════

interface UserContextType {
    customerId: string;
    isReady: boolean;
}

const UserContext = createContext<UserContextType | null>(null);

const STORAGE_KEY = 'flashsale_customer_id';

/**
 * Gera um UUID v4 no formato do .NET Guid
 */
function generateUUID(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = (Math.random() * 16) | 0;
        const v = c === 'x' ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}

interface UserProviderProps {
    children: ReactNode;
}

export function UserProvider({ children }: UserProviderProps) {
    const [customerId, setCustomerId] = useState<string>('');
    const [isReady, setIsReady] = useState(false);

    useEffect(() => {
        // Tentar recuperar do localStorage
        let storedId = localStorage.getItem(STORAGE_KEY);

        if (!storedId) {
            // Gerar novo ID
            storedId = generateUUID();
            localStorage.setItem(STORAGE_KEY, storedId);
            console.log('🆔 Novo CustomerId gerado:', storedId);
        } else {
            console.log('🆔 CustomerId recuperado:', storedId);
        }

        setCustomerId(storedId);
        setIsReady(true);
    }, []);

    return (
        <UserContext.Provider value={{ customerId, isReady }}>
            {children}
        </UserContext.Provider>
    );
}

export function useUser(): UserContextType {
    const context = useContext(UserContext);
    if (!context) {
        throw new Error('useUser deve ser usado dentro de UserProvider');
    }
    return context;
}
