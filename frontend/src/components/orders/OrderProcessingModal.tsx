'use client';

import { useState, useEffect } from 'react';
import { OrderStatus, OrderConfirmedEvent, OrderFailedEvent } from '@/types';

// ════════════════════════════════════════════════════════════════════════
// Modal de Processamento do Pedido
// Exibe "Aguarde..." e atualiza quando recebe SignalR event
// ════════════════════════════════════════════════════════════════════════

interface OrderProcessingModalProps {
    isOpen: boolean;
    orderId: string | null;
    status: 'pending' | 'processing' | 'confirmed' | 'failed';
    confirmedData?: OrderConfirmedEvent;
    failedData?: OrderFailedEvent;
    onClose: () => void;
    onViewStatus: (orderId: string) => void;
}

export function OrderProcessingModal({
    isOpen,
    orderId,
    status,
    confirmedData,
    failedData,
    onClose,
    onViewStatus
}: OrderProcessingModalProps) {
    const [dots, setDots] = useState('');

    // Animação de pontos
    useEffect(() => {
        if (status === 'pending' || status === 'processing') {
            const interval = setInterval(() => {
                setDots(prev => prev.length >= 3 ? '' : prev + '.');
            }, 500);
            return () => clearInterval(interval);
        }
    }, [status]);

    if (!isOpen || !orderId) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            {/* Overlay */}
            <div
                className="absolute inset-0 bg-black/70 backdrop-blur-sm"
                onClick={status !== 'pending' && status !== 'processing' ? onClose : undefined}
            />

            {/* Modal */}
            <div className="relative bg-gradient-to-br from-slate-800 to-slate-900 rounded-2xl border border-slate-700/50 p-8 max-w-md w-full mx-4 shadow-2xl">

                {/* Status: Pending/Processing */}
                {(status === 'pending' || status === 'processing') && (
                    <div className="text-center">
                        <div className="mb-6">
                            <div className="w-16 h-16 mx-auto border-4 border-indigo-500 border-t-transparent rounded-full animate-spin" />
                        </div>
                        <h2 className="text-2xl font-bold text-white mb-2">
                            Processando seu pedido{dots}
                        </h2>
                        <p className="text-slate-400 mb-4">
                            Aguarde, estamos verificando a disponibilidade.
                        </p>
                        <div className="bg-slate-800/50 rounded-lg p-3">
                            <p className="text-xs text-slate-500">ID do Pedido:</p>
                            <p className="text-sm text-indigo-400 font-mono">{orderId}</p>
                        </div>
                    </div>
                )}

                {/* Status: Confirmed */}
                {status === 'confirmed' && confirmedData && (
                    <div className="text-center">
                        <div className="mb-6">
                            <div className="w-16 h-16 mx-auto bg-green-500/20 rounded-full flex items-center justify-center">
                                <span className="text-4xl">✅</span>
                            </div>
                        </div>
                        <h2 className="text-2xl font-bold text-green-400 mb-2">
                            Pedido Confirmado!
                        </h2>
                        <p className="text-slate-400 mb-4">
                            Sua compra foi realizada com sucesso.
                        </p>
                        <div className="bg-green-500/10 border border-green-500/20 rounded-lg p-4 mb-6">
                            <p className="text-lg font-bold text-white">
                                Total: R$ {confirmedData.totalAmount.toFixed(2)}
                            </p>
                        </div>
                        <div className="flex gap-3">
                            <button
                                onClick={onClose}
                                className="flex-1 px-4 py-2 bg-slate-700 hover:bg-slate-600 text-white rounded-lg transition-colors"
                            >
                                Continuar Comprando
                            </button>
                            <button
                                onClick={() => onViewStatus(orderId)}
                                className="flex-1 px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg transition-colors"
                            >
                                Ver Pedido
                            </button>
                        </div>
                    </div>
                )}

                {/* Status: Failed */}
                {status === 'failed' && failedData && (
                    <div className="text-center">
                        <div className="mb-6">
                            <div className="w-16 h-16 mx-auto bg-red-500/20 rounded-full flex items-center justify-center">
                                <span className="text-4xl">❌</span>
                            </div>
                        </div>
                        <h2 className="text-2xl font-bold text-red-400 mb-2">
                            Pedido Não Confirmado
                        </h2>
                        <p className="text-slate-400 mb-4">
                            {failedData.reason || 'Não foi possível processar seu pedido.'}
                        </p>
                        <button
                            onClick={onClose}
                            className="w-full px-4 py-2 bg-slate-700 hover:bg-slate-600 text-white rounded-lg transition-colors"
                        >
                            Tentar Novamente
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}
