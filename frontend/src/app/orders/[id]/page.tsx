'use client';

import { useEffect, useState, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useSignalR } from '@/hooks/useSignalR';
import { getOrder } from '@/services/api';
import { OrderResponse, OrderConfirmedEvent, OrderFailedEvent } from '@/types';

// ════════════════════════════════════════════════════════════════════════
// Página de Status do Pedido
// Combina Polling + SignalR conforme INTEGRATION_PLAN.md
// ════════════════════════════════════════════════════════════════════════

export default function OrderStatusPage() {
    const params = useParams();
    const router = useRouter();
    const orderId = params.id as string;

    const [order, setOrder] = useState<OrderResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Handler para atualização SignalR
    const handleOrderConfirmed = useCallback((event: OrderConfirmedEvent) => {
        console.log('📦 Order Confirmed via SignalR:', event);
        if (event.orderId === orderId) {
            fetchOrder(); // Recarrega os dados completos
        }
    }, [orderId]);

    const handleOrderFailed = useCallback((event: OrderFailedEvent) => {
        console.log('❌ Order Failed via SignalR:', event);
        if (event.orderId === orderId) {
            fetchOrder();
        }
    }, [orderId]);

    // SignalR connection with order subscription
    const { isConnected, subscribeToOrder, unsubscribeFromOrder } = useSignalR({
        orderId,
        onOrderConfirmed: handleOrderConfirmed,
        onOrderFailed: handleOrderFailed,
    });

    // Fetch order data
    const fetchOrder = useCallback(async () => {
        try {
            const data = await getOrder(orderId);
            setOrder(data);
            setError(null);
        } catch (err) {
            console.error('Erro ao buscar pedido:', err);
            setError('Pedido não encontrado');
        } finally {
            setIsLoading(false);
        }
    }, [orderId]);

    // Initial load + Polling (only while pending)
    useEffect(() => {
        fetchOrder();

        // Polling a cada 2s enquanto status é Pending
        const pollInterval = setInterval(() => {
            if (order?.status === 'Pending' || order?.status === 'Processing') {
                fetchOrder();
            }
        }, 2000);

        return () => clearInterval(pollInterval);
    }, [fetchOrder, order?.status]);

    // Subscribe to SignalR when connected
    useEffect(() => {
        if (isConnected && orderId) {
            subscribeToOrder(orderId);
        }

        return () => {
            if (orderId) {
                unsubscribeFromOrder(orderId);
            }
        };
    }, [isConnected, orderId, subscribeToOrder, unsubscribeFromOrder]);

    // Status indicator
    const getStatusConfig = (status: string) => {
        switch (status) {
            case 'Pending':
                return { color: 'yellow', icon: '⏳', label: 'Aguardando Processamento' };
            case 'Processing':
                return { color: 'blue', icon: '⚙️', label: 'Em Processamento' };
            case 'Confirmed':
                return { color: 'green', icon: '✅', label: 'Confirmado' };
            case 'Failed':
                return { color: 'red', icon: '❌', label: 'Falhou' };
            case 'Cancelled':
                return { color: 'gray', icon: '🚫', label: 'Cancelado' };
            default:
                return { color: 'gray', icon: '❓', label: status };
        }
    };

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                    <div className="w-12 h-12 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
                    <p className="text-slate-400">Carregando pedido...</p>
                </div>
            </div>
        );
    }

    if (error || !order) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                    <div className="text-6xl mb-4">🔍</div>
                    <h1 className="text-2xl font-bold text-white mb-2">Pedido não encontrado</h1>
                    <p className="text-slate-400 mb-6">{error}</p>
                    <button
                        onClick={() => router.push('/')}
                        className="px-6 py-3 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg transition-colors"
                    >
                        Voltar para Ofertas
                    </button>
                </div>
            </div>
        );
    }

    const statusConfig = getStatusConfig(order.status);

    return (
        <div className="min-h-screen py-12">
            <div className="container mx-auto px-4 max-w-2xl">
                {/* Header */}
                <div className="mb-8">
                    <button
                        onClick={() => router.push('/')}
                        className="text-slate-400 hover:text-white transition-colors mb-4 flex items-center gap-2"
                    >
                        ← Voltar para Ofertas
                    </button>
                    <h1 className="text-3xl font-bold text-white">Status do Pedido</h1>
                </div>

                {/* Status Card */}
                <div className="bg-gradient-to-br from-slate-800 to-slate-900 rounded-2xl border border-slate-700/50 p-6 mb-6">
                    <div className="flex items-center gap-4 mb-6">
                        <div className={`w-16 h-16 rounded-full bg-${statusConfig.color}-500/20 flex items-center justify-center`}>
                            <span className="text-3xl">{statusConfig.icon}</span>
                        </div>
                        <div>
                            <p className="text-sm text-slate-400">Status</p>
                            <p className={`text-xl font-bold text-${statusConfig.color}-400`}>
                                {statusConfig.label}
                            </p>
                        </div>
                    </div>

                    {/* Order ID */}
                    <div className="bg-slate-800/50 rounded-lg p-4 mb-4">
                        <p className="text-xs text-slate-500 mb-1">ID do Pedido</p>
                        <p className="text-sm text-indigo-400 font-mono break-all">{order.id}</p>
                    </div>

                    {/* Total Amount */}
                    {order.status === 'Confirmed' && (
                        <div className="bg-green-500/10 border border-green-500/20 rounded-lg p-4 mb-4">
                            <p className="text-sm text-green-400 mb-1">Valor Total</p>
                            <p className="text-2xl font-bold text-white">
                                R$ {order.totalAmount.toFixed(2)}
                            </p>
                        </div>
                    )}

                    {/* Error Message */}
                    {order.errorMessage && (
                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 mb-4">
                            <p className="text-sm text-red-400 mb-1">Motivo da Falha</p>
                            <p className="text-white">{order.errorMessage}</p>
                        </div>
                    )}

                    {/* Timestamps */}
                    <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                            <p className="text-slate-500">Criado em</p>
                            <p className="text-white">
                                {new Date(order.createdAt).toLocaleString('pt-BR')}
                            </p>
                        </div>
                        {order.processedAt && (
                            <div>
                                <p className="text-slate-500">Processado em</p>
                                <p className="text-white">
                                    {new Date(order.processedAt).toLocaleString('pt-BR')}
                                </p>
                            </div>
                        )}
                    </div>
                </div>

                {/* Items */}
                {order.items.length > 0 && (
                    <div className="bg-gradient-to-br from-slate-800 to-slate-900 rounded-2xl border border-slate-700/50 p-6">
                        <h2 className="text-lg font-bold text-white mb-4">Itens do Pedido</h2>
                        <div className="space-y-3">
                            {order.items.map((item, index) => (
                                <div key={index} className="flex justify-between items-center p-3 bg-slate-800/50 rounded-lg">
                                    <div>
                                        <p className="text-white font-medium">{item.productName || 'Produto'}</p>
                                        <p className="text-sm text-slate-400">Qtd: {item.quantity}</p>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-white font-medium">R$ {item.subtotal.toFixed(2)}</p>
                                        <p className="text-xs text-slate-500">R$ {item.unitPrice.toFixed(2)} un.</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {/* SignalR Status */}
                <div className="mt-6 flex justify-center">
                    <div className={`flex items-center gap-2 px-3 py-2 rounded-full text-xs ${isConnected
                            ? 'bg-green-500/10 text-green-400 border border-green-500/20'
                            : 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20'
                        }`}>
                        <span className={`w-2 h-2 rounded-full ${isConnected ? 'bg-green-400' : 'bg-yellow-400 animate-pulse'}`} />
                        {isConnected ? 'Atualização em tempo real' : 'Conectando...'}
                    </div>
                </div>
            </div>
        </div>
    );
}
