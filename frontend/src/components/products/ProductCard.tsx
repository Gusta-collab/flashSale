'use client';

import { Product } from '@/types';

interface ProductCardProps {
    product: Product;
    onAddToCart: (product: Product) => void;
}

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
    const isOutOfStock = product.stock === 0;

    return (
        <div className="group relative overflow-hidden rounded-2xl bg-gradient-to-br from-slate-800/50 to-slate-900/50 backdrop-blur-sm border border-slate-700/50 hover:border-indigo-500/50 transition-all duration-300 hover:shadow-lg hover:shadow-indigo-500/10">
            {/* Badge de estoque */}
            {product.stock <= 5 && product.stock > 0 && (
                <div className="absolute top-3 right-3 z-10">
                    <span className="bg-amber-500/90 text-white text-xs font-semibold px-2 py-1 rounded-full animate-pulse">
                        Últimas {product.stock} unidades!
                    </span>
                </div>
            )}

            {isOutOfStock && (
                <div className="absolute inset-0 bg-black/60 z-10 flex items-center justify-center">
                    <span className="text-white text-lg font-bold">Esgotado</span>
                </div>
            )}

            {/* Imagem placeholder */}
            <div className="h-48 bg-gradient-to-br from-indigo-500/20 to-purple-500/20 flex items-center justify-center group-hover:scale-105 transition-transform duration-300">
                <span className="text-6xl">🛒</span>
            </div>

            {/* Conteúdo */}
            <div className="p-5">
                <h3 className="text-lg font-semibold text-white mb-1 truncate group-hover:text-indigo-400 transition-colors">
                    {product.name}
                </h3>

                {product.description && (
                    <p className="text-slate-400 text-sm mb-3 line-clamp-2">
                        {product.description}
                    </p>
                )}

                <div className="flex items-center justify-between">
                    <div>
                        <span className="text-2xl font-bold text-white">
                            R$ {product.price.toFixed(2)}
                        </span>
                    </div>

                    <button
                        onClick={() => onAddToCart(product)}
                        disabled={isOutOfStock}
                        className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-700 disabled:cursor-not-allowed text-white font-medium rounded-lg transition-all duration-200 transform hover:scale-105 active:scale-95"
                    >
                        {isOutOfStock ? 'Esgotado' : 'Comprar'}
                    </button>
                </div>
            </div>
        </div>
    );
}
