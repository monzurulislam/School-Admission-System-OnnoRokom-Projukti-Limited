import React, { useEffect, useState } from 'react'
import { Clock } from 'lucide-react'

interface HoldCountdownProps {
  expiresAt: string
  onExpire?: () => void
}

export const HoldCountdown: React.FC<HoldCountdownProps> = ({ expiresAt, onExpire }) => {
  const calculateRemaining = () => {
    const diff = Math.floor((new Date(expiresAt).getTime() - Date.now()) / 1000)
    return Math.max(0, diff)
  }

  const [remaining, setRemaining] = useState<number>(calculateRemaining)

  useEffect(() => {
    setRemaining(calculateRemaining())

    const interval = setInterval(() => {
      const current = calculateRemaining()
      setRemaining(current)
      if (current <= 0) {
        clearInterval(interval)
        onExpire?.()
      }
    }, 1000)

    return () => clearInterval(interval)
  }, [expiresAt])

  if (remaining <= 0) {
    return (
      <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-red-100 text-red-700">
        <Clock className="w-3.5 h-3.5" /> Expired (মেয়াদোত্তীর্ণ)
      </span>
    )
  }

  const minutes = Math.floor(remaining / 60)
  const seconds = remaining % 60
  const formatted = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`

  return (
    <span className="inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1 rounded-full bg-amber-50 text-amber-700 border border-amber-200">
      <Clock className="w-3.5 h-3.5 animate-pulse text-amber-600" />
      <span>{formatted} remaining</span>
    </span>
  )
}

