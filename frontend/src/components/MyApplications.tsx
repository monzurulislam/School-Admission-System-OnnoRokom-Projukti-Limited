import React, { useState } from 'react'
import { api } from '../api/client'
import type { StudentHoldItem } from '../api/client'
import { HoldCountdown } from './HoldCountdown'
import { CheckCircle2, XCircle, Clock, CreditCard, Ban, AlertCircle } from 'lucide-react'

interface MyApplicationsProps {
  holds: StudentHoldItem[]
  studentId: string
  onRefresh: () => void
}

export const MyApplications: React.FC<MyApplicationsProps> = ({ holds, studentId, onRefresh }) => {
  const [actionLoading, setActionLoading] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const handleConfirm = async (holdId: string) => {
    try {
      setActionLoading(holdId)
      setError(null)
      setSuccess(null)
      await api.confirmHold(holdId, studentId)
      setSuccess('Admission confirmed successfully! Congratulations. (ভর্তি নিশ্চিতকরণ সম্পন্ন হয়েছে)')
      onRefresh()
    } catch (err: any) {
      setError(err.message || 'Failed to confirm admission')
    } finally {
      setActionLoading(null)
    }
  }

  const handleCancel = async (holdId: string) => {
    if (!window.confirm('Are you sure you want to cancel this hold? The seat will be released immediately.')) {
      return
    }

    try {
      setActionLoading(holdId)
      setError(null)
      setSuccess(null)
      await api.cancelHold(holdId, studentId)
      setSuccess('Hold cancelled and seat released. (আসনটি সফলভাবে মুক্ত করা হয়েছে)')
      onRefresh()
    } catch (err: any) {
      setError(err.message || 'Failed to cancel hold')
    } finally {
      setActionLoading(null)
    }
  }

  if (holds.length === 0) {
    return (
      <div className="empty-state-card">
        <Clock className="w-10 h-10 text-slate-300 mx-auto mb-2" />
        <h4 className="text-sm font-semibold text-slate-700">No applications found</h4>
        <p className="text-xs text-slate-500 max-w-sm mx-auto mt-1">
          Select an available class from the list above and click &quot;Apply for Seat&quot; to begin your admission process.
        </p>
      </div>
    )
  }

  return (
    <div className="my-applications-wrapper">
      {error && (
        <div className="alert-error flex items-center gap-2 mb-4">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {success && (
        <div className="alert-success flex items-center gap-2 mb-4">
          <CheckCircle2 className="w-4 h-4 shrink-0 text-emerald-600" />
          <span>{success}</span>
        </div>
      )}

      <div className="applications-list">
        {holds.map((h) => {
          const isHeld = h.status === 'HELD'
          const isConfirmed = h.status === 'CONFIRMED'
          const isExpired = h.status === 'EXPIRED'
          const isCancelled = h.status === 'CANCELLED'

          return (
            <div key={h.holdId} className={`application-card application-card-${h.status.toLowerCase()}`}>
              <div className="app-card-header">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="app-hold-id">{h.holdId}</span>
                    <span className="text-xs text-slate-400">• Class ID: {h.classId}</span>
                  </div>
                  <h4 className="app-class-name">{h.className || `Class ${h.classId}`}</h4>
                </div>

                <div className="app-status-badge">
                  {isConfirmed && (
                    <span className="badge-confirmed flex items-center gap-1">
                      <CheckCircle2 className="w-3.5 h-3.5" /> CONFIRMED (ভর্তি সম্পন্ন)
                    </span>
                  )}
                  {isHeld && (
                    <span className="badge-held flex items-center gap-1">
                      <Clock className="w-3.5 h-3.5" /> HELD (আসন সংরক্ষিত)
                    </span>
                  )}
                  {isExpired && (
                    <span className="badge-expired flex items-center gap-1">
                      <XCircle className="w-3.5 h-3.5" /> EXPIRED (মেয়াদ শেষ)
                    </span>
                  )}
                  {isCancelled && (
                    <span className="badge-cancelled flex items-center gap-1">
                      <Ban className="w-3.5 h-3.5" /> CANCELLED (বাতিল)
                    </span>
                  )}
                </div>
              </div>

              {/* Body: Countdown and Payment Simulation */}
              <div className="app-card-body">
                {isHeld && (
                  <div className="flex flex-wrap items-center justify-between gap-3 bg-amber-50/60 p-3 rounded-lg border border-amber-100">
                    <div>
                      <p className="text-xs font-medium text-amber-900">
                        Temporary Hold Active (আসন সংরক্ষণ সময়সীমা):
                      </p>
                      <p className="text-[11px] text-amber-700 mt-0.5">
                        Complete payment before the timer expires to secure admission.
                      </p>
                    </div>
                    <HoldCountdown expiresAt={h.expiresAt} onExpire={onRefresh} />
                  </div>
                )}

                {isConfirmed && (
                  <div className="bg-emerald-50/70 p-3 rounded-lg border border-emerald-100 flex items-center gap-2">
                    <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />
                    <p className="text-xs text-emerald-800 font-medium">
                      Admission successfully secured and verified. Welcome to the class!
                    </p>
                  </div>
                )}
              </div>

              {/* Actions */}
              {isHeld && (
                <div className="app-card-actions">
                  <button
                    type="button"
                    onClick={() => handleCancel(h.holdId)}
                    disabled={actionLoading === h.holdId}
                    className="btn-danger-outline text-xs"
                  >
                    <Ban className="w-3.5 h-3.5" /> Cancel Application
                  </button>

                  <button
                    type="button"
                    onClick={() => handleConfirm(h.holdId)}
                    disabled={actionLoading === h.holdId}
                    className="btn-pay text-xs"
                  >
                    <CreditCard className="w-3.5 h-3.5" />
                    {actionLoading === h.holdId ? 'Confirming...' : 'Pay Admission Fee (ফি পরিশোধ করুন)'}
                  </button>
                </div>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}
