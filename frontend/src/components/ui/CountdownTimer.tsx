'use client';

import { useEffect, useState } from 'react';

interface CountdownTimerProps {
    endTime: Date;
    onEnd?: () => void;
}

export function CountdownTimer({ endTime, onEnd }: CountdownTimerProps) {
    const [timeLeft, setTimeLeft] = useState(calculateTimeLeft());

    function calculateTimeLeft() {
        const difference = endTime.getTime() - Date.now();

        if (difference <= 0) {
            return { hours: 0, minutes: 0, seconds: 0 };
        }

        return {
            hours: Math.floor((difference / (1000 * 60 * 60)) % 24),
            minutes: Math.floor((difference / 1000 / 60) % 60),
            seconds: Math.floor((difference / 1000) % 60),
        };
    }

    useEffect(() => {
        const timer = setInterval(() => {
            const newTimeLeft = calculateTimeLeft();
            setTimeLeft(newTimeLeft);

            if (newTimeLeft.hours === 0 && newTimeLeft.minutes === 0 && newTimeLeft.seconds === 0) {
                onEnd?.();
                clearInterval(timer);
            }
        }, 1000);

        return () => clearInterval(timer);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [endTime]);

    const isUrgent = timeLeft.hours === 0 && timeLeft.minutes < 10;

    return (
        <div className={`flex items-center gap-4 ${isUrgent ? 'animate-pulse' : ''}`}>
            <span className="text-slate-400 text-sm uppercase tracking-wider">Termina em:</span>

            <div className="flex gap-2">
                <TimeBlock value={timeLeft.hours} label="HRS" isUrgent={isUrgent} />
                <span className="text-2xl text-slate-500 font-bold">:</span>
                <TimeBlock value={timeLeft.minutes} label="MIN" isUrgent={isUrgent} />
                <span className="text-2xl text-slate-500 font-bold">:</span>
                <TimeBlock value={timeLeft.seconds} label="SEG" isUrgent={isUrgent} />
            </div>
        </div>
    );
}

function TimeBlock({ value, label, isUrgent }: { value: number; label: string; isUrgent: boolean }) {
    return (
        <div className={`flex flex-col items-center justify-center w-16 h-16 rounded-lg ${isUrgent
            ? 'bg-red-500/20 border border-red-500/50'
            : 'bg-slate-800/50 border border-slate-700/50'
            }`}>
            <span className={`text-2xl font-bold ${isUrgent ? 'text-red-400' : 'text-white'}`}>
                {String(value).padStart(2, '0')}
            </span>
            <span className="text-[10px] text-slate-500 uppercase">{label}</span>
        </div>
    );
}
