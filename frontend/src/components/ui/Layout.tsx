export function Header() {
    return (
        <header className="sticky top-0 z-40 w-full backdrop-blur-lg bg-slate-900/80 border-b border-slate-800">
            <div className="container mx-auto px-4">
                <div className="flex items-center justify-between h-16">
                    {/* Logo */}
                    <div className="flex items-center gap-2">
                        <span className="text-2xl">⚡</span>
                        <span className="text-xl font-bold bg-gradient-to-r from-indigo-400 to-purple-400 bg-clip-text text-transparent">
                            FlashSale
                        </span>
                    </div>

                    {/* Nav */}
                    <nav className="hidden md:flex items-center gap-6">
                        <a href="/" className="text-slate-300 hover:text-white transition-colors">
                            Ofertas
                        </a>
                        <a href="#" className="text-slate-300 hover:text-white transition-colors">
                            Meus Pedidos
                        </a>
                    </nav>

                    {/* Status */}
                    <div className="flex items-center gap-3">
                        <div className="flex items-center gap-2 px-3 py-1.5 rounded-full bg-green-500/10 border border-green-500/20">
                            <span className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></span>
                            <span className="text-sm text-green-400">Ao vivo</span>
                        </div>
                    </div>
                </div>
            </div>
        </header>
    );
}

export function Footer() {
    return (
        <footer className="border-t border-slate-800 bg-slate-900/50 mt-auto">
            <div className="container mx-auto px-4 py-8">
                <div className="flex flex-col md:flex-row items-center justify-between gap-4">
                    <div className="flex items-center gap-2">
                        <span className="text-xl">⚡</span>
                        <span className="text-sm text-slate-400">
                            FlashSale © 2026 - Sistema de Vendas de Alta Demanda
                        </span>
                    </div>

                    <div className="flex items-center gap-4 text-sm text-slate-500">
                        <span>Powered by Redis Streams + SignalR</span>
                    </div>
                </div>
            </div>
        </footer>
    );
}
