import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Header, Footer } from "@/components/ui/Layout";
import { UserProvider } from "@/contexts/UserContext";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "FlashSale - Ofertas Relâmpago",
  description: "Sistema de vendas de alta demanda com processamento em tempo real",
  keywords: ["flash sale", "ofertas", "promoção", "desconto"],
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" className="dark">
      <body className={`${inter.className} bg-slate-950 text-white min-h-screen flex flex-col`}>
        <UserProvider>
          <Header />
          <main className="flex-1">
            {children}
          </main>
          <Footer />
        </UserProvider>
      </body>
    </html>
  );
}
