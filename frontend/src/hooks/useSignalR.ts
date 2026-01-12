'use client';

import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { OrderConfirmedEvent, OrderFailedEvent } from '@/types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

interface UseSignalROptions {
    orderId?: string;
    onOrderConfirmed?: (event: OrderConfirmedEvent) => void;
    onOrderFailed?: (event: OrderFailedEvent) => void;
    onStatusChanged?: (event: { orderId: string; status: string }) => void;
}

export function useSignalR(options: UseSignalROptions = {}) {
    const [isConnected, setIsConnected] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    const connect = useCallback(async () => {
        if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE_URL}/hubs/orders`)
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Event handlers
        connection.on('OrderConfirmed', (event: OrderConfirmedEvent) => {
            console.log('📦 Order Confirmed:', event);
            options.onOrderConfirmed?.(event);
        });

        connection.on('OrderFailed', (event: OrderFailedEvent) => {
            console.log('❌ Order Failed:', event);
            options.onOrderFailed?.(event);
        });

        connection.on('OrderStatusChanged', (event: { orderId: string; status: string }) => {
            console.log('🔄 Status Changed:', event);
            options.onStatusChanged?.(event);
        });

        connection.onclose(() => {
            setIsConnected(false);
            console.log('🔌 SignalR disconnected');
        });

        connection.onreconnected(() => {
            setIsConnected(true);
            console.log('🔌 SignalR reconnected');

            // Re-subscribe to order if we have one
            if (options.orderId) {
                subscribeToOrder(options.orderId);
            }
        });

        try {
            await connection.start();
            connectionRef.current = connection;
            setIsConnected(true);
            setError(null);
            console.log('🔌 SignalR connected');

            // Subscribe to order if provided
            if (options.orderId) {
                await subscribeToOrder(options.orderId);
            }
        } catch (err) {
            console.error('SignalR connection error:', err);
            setError('Falha ao conectar com notificações em tempo real');
        }
    }, [options.orderId]);

    const subscribeToOrder = useCallback(async (orderId: string) => {
        if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
            return;
        }

        try {
            await connectionRef.current.invoke('SubscribeToOrder', orderId);
            console.log(`📡 Subscribed to order: ${orderId}`);
        } catch (err) {
            console.error('Failed to subscribe:', err);
        }
    }, []);

    const unsubscribeFromOrder = useCallback(async (orderId: string) => {
        if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
            return;
        }

        try {
            await connectionRef.current.invoke('UnsubscribeFromOrder', orderId);
            console.log(`📡 Unsubscribed from order: ${orderId}`);
        } catch (err) {
            console.error('Failed to unsubscribe:', err);
        }
    }, []);

    const disconnect = useCallback(async () => {
        if (connectionRef.current) {
            await connectionRef.current.stop();
            connectionRef.current = null;
            setIsConnected(false);
        }
    }, []);

    useEffect(() => {
        connect();
        return () => {
            disconnect();
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return {
        isConnected,
        error,
        connect,
        disconnect,
        subscribeToOrder,
        unsubscribeFromOrder,
    };
}
