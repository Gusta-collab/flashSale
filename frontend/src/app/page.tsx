'use client';

import { useState, useEffect } from 'react';
import { ProductCard } from '@/components/products/ProductCard';
import { CountdownTimer } from '@/components/ui/CountdownTimer';
import { ToastContainer, useToast } from '@/components/ui/Toast';
import { useSignalR } from '@/hooks/useSignalR';
import { getProducts, createOrder, generateIdempotencyKey } from '@/services/api';
import { Product } from '@/types';

// Dados mock para desenvolvimento (quando backend não está rodando)
const mockProducts: Product[] = [
  { id: '1', name: 'iPhone 15 Pro Max', description: 'O smartphone mais avançado da Apple', price: 8999.00, stock: 5, isActive: true },
  { id: '2', name: 'Samsung Galaxy S24 Ultra', description: 'Potência e inteligência artificial', price: 7499.00, stock: 8, isActive: true },
  { id: '3', name: 'MacBook Pro M3', description: 'Performance profissional', price: 14999.00, stock: 3, isActive: true },
  { id: '4', name: 'PlayStation 5', description: 'A nova geração de games', price: 3999.00, stock: 0, isActive: true },
  { id: '5', name: 'AirPods Pro 2', description: 'Som imersivo e cancelamento de ruído', price: 1899.00, stock: 15, isActive: true },
  { id: '6', name: 'iPad Pro 12.9"', description: 'Seu próximo computador não é um computador', price: 9499.00, stock: 4, isActive: true },
];

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>(mockProducts);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isOrdering, setIsOrdering] = useState(false);

  const { toasts, addToast, removeToast } = useToast();

  // SignalR para notificações
  const { isConnected } = useSignalR({
    onOrderConfirmed: (event) => {
      addToast({
        type: 'success',
        title: '✅ Pedido Confirmado!',
        message: `Valor total: R$ ${event.totalAmount.toFixed(2)}`,
      });
      setIsOrdering(false);
      setSelectedProduct(null);
    },
    onOrderFailed: (event) => {
      addToast({
        type: 'error',
        title: '❌ Pedido Falhou',
        message: event.reason,
      });
      setIsOrdering(false);
    },
  });

  // Flash Sale termina em 2 horas
  const flashSaleEndTime = new Date(Date.now() + 2 * 60 * 60 * 1000);

  useEffect(() => {
    async function loadProducts() {
      try {
        const data = await getProducts();
        setProducts(data);
      } catch (error) {
        console.log('Backend não disponível, usando mock data');
        // Mantém mockProducts
      } finally {
        setIsLoading(false);
      }
    }
    loadProducts();
  }, []);

  async function handleBuy(product: Product) {
    setSelectedProduct(product);
    setIsOrdering(true);

    try {
      const response = await createOrder({
        customerId: 'demo-customer-123',
        idempotencyKey: generateIdempotencyKey(),
        items: [{ productId: product.id, quantity: 1 }],
      });

      addToast({
        type: 'info',
        title: '⏳ Pedido Recebido',
        message: 'Processando seu pedido...',
      });

      // Atualizar estoque local
      setProducts(prev => prev.map(p =>
        p.id === product.id ? { ...p, stock: p.stock - 1 } : p
      ));

    } catch (error) {
      addToast({
        type: 'error',
        title: 'Erro ao criar pedido',
        message: 'Tente novamente em instantes',
      });
      setIsOrdering(false);
      setSelectedProduct(null);
    }
  }

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative py-16 overflow-hidden">
        {/* Background gradient */}
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

            {/* Countdown */}
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

      {/* Toasts */}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </div>
  );
}
