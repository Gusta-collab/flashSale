'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { ProductCard } from '@/components/products/ProductCard';
import { CountdownTimer } from '@/components/ui/CountdownTimer';
import { ToastContainer, useToast } from '@/components/ui/Toast';
import { OrderProcessingModal } from '@/components/orders/OrderProcessingModal';
import { useSignalR } from '@/hooks/useSignalR';
import { useUser } from '@/contexts/UserContext';
import { getProducts, createOrder, generateIdempotencyKey, extractUtmParams } from '@/services/api';
import { Product, OrderConfirmedEvent, OrderFailedEvent } from '@/types';

// ════════════════════════════════════════════════════════════════════════
// Home Page - Flash Sale
// Fluxo conforme docs:
// 1. Cliente clica "Comprar"
// 2. POST /orders → Retorna 202 + orderId
// 3. Modal "Aguardando..." + SubscribeToOrder(orderId)
// 4. Recebe OrderConfirmed/Failed via SignalR
// ════════════════════════════════════════════════════════════════════════

// Mock products para quando backend não está disponível
const mockProducts: Product[] = [
  { id: 'f47ac10b-58cc-4372-a567-0e02b2c3d479', name: 'iPhone 15 Pro Max', description: 'O smartphone mais avançado da Apple', price: 8999.00, stock: 5, isActive: true },
  { id: '550e8400-e29b-41d4-a716-446655440001', name: 'Samsung Galaxy S24 Ultra', description: 'Potência e inteligência artificial', price: 7499.00, stock: 8, isActive: true },
  { id: '550e8400-e29b-41d4-a716-446655440002', name: 'MacBook Pro M3', description: 'Performance profissional', price: 14999.00, stock: 3, isActive: true },
  { id: '550e8400-e29b-41d4-a716-446655440003', name: 'PlayStation 5', description: 'A nova geração de games', price: 3999.00, stock: 0, isActive: true },
  { id: '550e8400-e29b-41d4-a716-446655440004', name: 'AirPods Pro 2', description: 'Som imersivo e cancelamento de ruído', price: 1899.00, stock: 15, isActive: true },
  { id: '550e8400-e29b-41d4-a716-446655440005', name: 'iPad Pro 12.9"', description: 'Seu próximo computador não é um computador', price: 9499.00, stock: 4, isActive: true },
];

export default function HomePage() {
  const router = useRouter();
  const { customerId, isReady } = useUser();
  const { toasts, addToast, removeToast } = useToast();

  // State
  const [products, setProducts] = useState<Product[]>(mockProducts);
  const [isLoading, setIsLoading] = useState(true);

  // Order processing state
  const [currentOrderId, setCurrentOrderId] = useState<string | null>(null);
  const [orderStatus, setOrderStatus] = useState<'pending' | 'processing' | 'confirmed' | 'failed'>('pending');
  const [confirmedData, setConfirmedData] = useState<OrderConfirmedEvent | undefined>();
  const [failedData, setFailedData] = useState<OrderFailedEvent | undefined>();
  const [showModal, setShowModal] = useState(false);

  // SignalR event handlers
  const handleOrderConfirmed = useCallback((event: OrderConfirmedEvent) => {
    console.log('📦 OrderConfirmed recebido:', event);
    if (event.orderId === currentOrderId) {
      setOrderStatus('confirmed');
      setConfirmedData(event);

      // Atualizar estoque local
      setProducts(prev => prev.map(p => ({
        ...p,
        stock: Math.max(0, p.stock - 1) // Simplificado - idealmente recarregar do backend
      })));
    }
  }, [currentOrderId]);

  const handleOrderFailed = useCallback((event: OrderFailedEvent) => {
    console.log('❌ OrderFailed recebido:', event);
    if (event.orderId === currentOrderId) {
      setOrderStatus('failed');
      setFailedData(event);
    }
  }, [currentOrderId]);

  // SignalR connection
  const { isConnected, subscribeToOrder, unsubscribeFromOrder } = useSignalR({
    orderId: currentOrderId || undefined,
    onOrderConfirmed: handleOrderConfirmed,
    onOrderFailed: handleOrderFailed,
  });

  // Flash Sale termina em 2 horas
  const flashSaleEndTime = new Date(Date.now() + 2 * 60 * 60 * 1000);

  // Load products
  useEffect(() => {
    async function loadProducts() {
      try {
        const data = await getProducts();
        setProducts(data);
      } catch (error) {
        console.log('Backend não disponível, usando mock data');
      } finally {
        setIsLoading(false);
      }
    }
    loadProducts();
  }, []);

  // Subscribe to order when orderId changes
  useEffect(() => {
    if (isConnected && currentOrderId) {
      subscribeToOrder(currentOrderId);
      console.log('📡 Subscribed to order:', currentOrderId);
    }

    return () => {
      if (currentOrderId) {
        unsubscribeFromOrder(currentOrderId);
      }
    };
  }, [isConnected, currentOrderId, subscribeToOrder, unsubscribeFromOrder]);

  // Handle buy click
  async function handleBuy(product: Product) {
    if (!isReady || !customerId) {
      addToast({
        type: 'error',
        title: 'Erro',
        message: 'Aguarde a inicialização...',
      });
      return;
    }

    // Reset state
    setOrderStatus('pending');
    setConfirmedData(undefined);
    setFailedData(undefined);

    try {
      // Extrair UTM params
      const utmParams = extractUtmParams();

      // POST /orders → 202 Accepted
      const response = await createOrder({
        customerId,
        idempotencyKey: generateIdempotencyKey(),
        items: [{ productId: product.id, quantity: 1 }],
        ...utmParams,
      });

      console.log('📝 Pedido aceito:', response);

      // Abrir modal e aguardar SignalR
      setCurrentOrderId(response.orderId);
      setShowModal(true);

      addToast({
        type: 'info',
        title: '⏳ Pedido na Fila',
        message: response.message,
      });

    } catch (error: unknown) {
      console.error('Erro ao criar pedido:', error);
      addToast({
        type: 'error',
        title: 'Erro ao criar pedido',
        message: 'Tente novamente em instantes',
      });
    }
  }

  function handleCloseModal() {
    setShowModal(false);
    setCurrentOrderId(null);
    setOrderStatus('pending');
  }

  function handleViewOrderStatus(orderId: string) {
    setShowModal(false);
    router.push(`/orders/${orderId}`);
  }

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative py-16 overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-b from-indigo-500/10 via-transparent to-transparent pointer-events-none" />

        <div className="container mx-auto px-4 relative">
          <div className="text-center mb-12">
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-indigo-500/10 border border-indigo-500/20 mb-6">
              <span className="w-2 h-2 bg-red-500 rounded-full animate-pulse"></span>
              <span className="text-sm text-indigo-300 font-medium">Flash Sale Ativo Agora</span>
            </div>

            <h1 className="text-4xl md:text-6xl font-bold mb-4">
              <span className="bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400 bg-clip-text text-transparent">
                Ofertas Relâmpago
              </span>
            </h1>

            <p className="text-slate-400 text-lg max-w-2xl mx-auto mb-8">
              Descontos exclusivos por tempo limitado. Quando acabar, acabou!
            </p>

            <div className="flex justify-center">
              <CountdownTimer
                endTime={flashSaleEndTime}
                onEnd={() => addToast({ type: 'info', title: 'Flash Sale encerrado!' })}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Products Grid */}
      <section className="py-12">
        <div className="container mx-auto px-4">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-2xl font-bold text-white">Produtos em Destaque</h2>
            <span className="text-sm text-slate-400">
              {products.filter(p => p.stock > 0).length} produtos disponíveis
            </span>
          </div>

          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[...Array(6)].map((_, i) => (
                <div key={i} className="h-80 rounded-2xl bg-slate-800/50 animate-pulse" />
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {products.map((product) => (
                <ProductCard
                  key={product.id}
                  product={product}
                  onAddToCart={handleBuy}
                />
              ))}
            </div>
          )}
        </div>
      </section>

      {/* Connection Status */}
      <div className="fixed bottom-4 left-4">
        <div className={`flex items-center gap-2 px-3 py-2 rounded-full text-xs ${isConnected
            ? 'bg-green-500/10 text-green-400 border border-green-500/20'
            : 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20'
          }`}>
          <span className={`w-2 h-2 rounded-full ${isConnected ? 'bg-green-400' : 'bg-yellow-400 animate-pulse'}`} />
          {isConnected ? 'Conectado' : 'Conectando...'}
        </div>
      </div>

      {/* Order Processing Modal */}
      <OrderProcessingModal
        isOpen={showModal}
        orderId={currentOrderId}
        status={orderStatus}
        confirmedData={confirmedData}
        failedData={failedData}
        onClose={handleCloseModal}
        onViewStatus={handleViewOrderStatus}
      />

      {/* Toasts */}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </div>
  );
}
