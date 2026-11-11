import React from 'react'
import { Users, CheckCircle2, Clock, AlertTriangle, ArrowRight } from 'lucide-react'
import type { ClassItem } from '../api/client'

interface ClassCardProps {
  classItem: ClassItem
  studentId: string
  hasActiveHoldInThisClass: boolean
  hasConfirmedInThisClass: boolean
  onApply: (classId: string) => void
  isApplying: boolean
}

export const ClassCard: React.FC<ClassCardProps> = ({
  classItem,
  studentId,
  hasActiveHoldInThisClass,
  hasConfirmedInThisClass,
  onApply,
  isApplying,
}) => {
  const isStudentValid = /^\d{17}$/.test(studentId)
  const isFull = classItem.availableSeats <= 0

  // Calculate percentages for the progress bar
  const confirmedPct = Math.min(100, (classItem.confirmedSeats / classItem.totalSeats) * 100)
  const heldPct = Math.min(100 - confirmedPct, (classItem.heldSeats / classItem.totalSeats) * 100)
  const availablePct = Math.max(0, 100 - (confirmedPct + heldPct))

  let buttonDisabled = false
  let buttonText = 'Apply for Seat (আসন গ্রহণ)'
  let buttonHint = ''

  if (!isStudentValid) {
    buttonDisabled = true
    buttonHint = 'Enter 17-digit student ID above to apply'
  } else if (hasConfirmedInThisClass) {
    buttonDisabled = true
    buttonText = 'Enrolled (ভর্তি সম্পন্ন)'
    buttonHint = 'Student already has confirmed admission in this class'
  } else if (hasActiveHoldInThisClass) {
    buttonDisabled = true
    buttonText = 'Seat Held (হোল্ড করা হয়েছে)'
    buttonHint = 'You currently have an active hold in this class'
  } else if (isFull) {
    buttonDisabled = true
    buttonText = 'Class Full (আসন নেই)'
    buttonHint = 'No seats currently available'
  }

  return (
    <div className={`class-card ${isFull ? 'class-card-full' : ''}`}>
      <div className="class-card-header">
        <div>
          <span className="class-id-badge">{classItem.id}</span>
          <h3 className="class-title">{classItem.name}</h3>
        </div>
        <div className="class-status-pill">
          {isFull ? (
            <span className="status-pill-full">
              <AlertTriangle className="w-3.5 h-3.5" /> Full
            </span>
          ) : (
            <span className="status-pill-available">
              {classItem.availableSeats} Available
            </span>
          )}
        </div>
      </div>

      {/* Progress Bar of Seat Allocation */}
      <div className="seat-progress-container">
        <div className="seat-progress-bar">
          <div
            className="seat-fill-confirmed"
            style={{ width: `${confirmedPct}%` }}
            title={`Confirmed: ${classItem.confirmedSeats}`}
          />
          <div
            className="seat-fill-held"
            style={{ width: `${heldPct}%` }}
            title={`Held: ${classItem.heldSeats}`}
          />
          <div
            className="seat-fill-available"
            style={{ width: `${availablePct}%` }}
            title={`Available: ${classItem.availableSeats}`}
          />
        </div>
      </div>

      {/* Numerical Seat Breakdown Grid */}
      <div className="seat-breakdown-grid">
        <div className="seat-stat-box">
          <span className="stat-label flex items-center gap-1">
            <Users className="w-3 h-3 text-slate-400" /> Total
          </span>
          <span className="stat-value text-slate-800">{classItem.totalSeats}</span>
        </div>
        <div className="seat-stat-box">
          <span className="stat-label flex items-center gap-1">
            <CheckCircle2 className="w-3 h-3 text-emerald-500" /> Confirmed
          </span>
          <span className="stat-value text-emerald-700">{classItem.confirmedSeats}</span>
        </div>
        <div className="seat-stat-box">
          <span className="stat-label flex items-center gap-1">
            <Clock className="w-3 h-3 text-amber-500" /> Held
          </span>
          <span className="stat-value text-amber-700">{classItem.heldSeats}</span>
        </div>
        <div className="seat-stat-box">
          <span className="stat-label flex items-center gap-1">
            <span className="w-2 h-2 rounded-full bg-blue-500 inline-block" /> Available
          </span>
          <span className="stat-value text-blue-700">{classItem.availableSeats}</span>
        </div>
      </div>

      {/* Apply Button */}
      <div className="class-card-footer">
        <button
          type="button"
          onClick={() => onApply(classItem.id)}
          disabled={buttonDisabled || isApplying}
          className={`btn-apply ${buttonDisabled ? 'btn-apply-disabled' : 'btn-apply-active'}`}
          title={buttonHint}
        >
          <span>{isApplying ? 'Holding Seat...' : buttonText}</span>
          {!buttonDisabled && !isApplying && <ArrowRight className="w-4 h-4 ml-1" />}
        </button>
        {buttonHint && <p className="button-hint-text">{buttonHint}</p>}
      </div>
    </div>
  )
}
