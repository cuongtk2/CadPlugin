(defun c:inpdf ( /
                     mayin khogiay ctb ss i n ent vlaObj
                     minPt maxPt p1 p2 p1_2d p2_2d sa1 sa2
                     acad doc layout plotObj rot
                     oldBg oldCmd oldDia oldOsm
                     dx dy plotWindow
                   )
  (vl-load-com)

  ;; Ham noi bo tinh do lech truc X
  (defun dx (pt1 pt2)
    (abs (- (car pt1) (car pt2)))
  )

  ;; Ham noi bo tinh do lech truc Y
  (defun dy (pt1 pt2)
    (abs (- (cadr pt1) (cadr pt2)))
  )

  ;; Ham noi bo thiet lap va in vung cua so bang VLA
  (defun plotWindow (printer paper styleTable ptA ptB /
                     locAcad locDoc locLayout locPlot locRot
                     locP1 locP2 locSa1 locSa2
                   )
    (setq locAcad   (vlax-get-acad-object)
          locDoc    (vla-get-ActiveDocument locAcad)
          locLayout (vla-get-ActiveLayout locDoc)
          locPlot   (vla-get-Plot locDoc)
    )

    (if (< (dx ptA ptB) (dy ptA ptB))
      (setq locRot ac90degrees)
      (setq locRot ac0degrees)
    )

    (vla-put-ConfigName locLayout printer)
    (vla-put-CanonicalMediaName locLayout paper)
    (vla-put-PaperUnits locLayout acMillimeters)
    (vla-put-PlotRotation locLayout locRot)
    (vla-put-PlotType locLayout acWindow)
    (vla-put-StandardScale locLayout acScaleToFit)
    (vla-put-UseStandardScale locLayout :vlax-true)
    (vla-put-CenterPlot locLayout :vlax-true)
    (vla-put-PlotWithPlotStyles locLayout :vlax-true)
    (vla-put-PlotWithLineweights locLayout :vlax-true)
    (vla-put-PlotHidden locLayout :vlax-false)

    (if (and styleTable (/= styleTable ""))
      (vla-put-StyleSheet locLayout styleTable)
    )

    (setq locP1  (list (float (car ptA)) (float (cadr ptA)))
          locP2  (list (float (car ptB)) (float (cadr ptB)))
          locSa1 (vlax-make-safearray vlax-vbDouble '(0 . 1))
          locSa2 (vlax-make-safearray vlax-vbDouble '(0 . 1))
    )
    (vlax-safearray-fill locSa1 locP1)
    (vlax-safearray-fill locSa2 locP2)
    (vla-SetWindowToPlot locLayout locSa1 locSa2)

    (vla-RefreshPlotDeviceInfo locLayout)
    (vla-PlotToDevice locPlot printer)
  )

  ;; Luu trang thai bien he thong
  (setq oldBg  (getvar "BACKGROUNDPLOT")
        oldCmd (getvar "CMDECHO")
        oldDia (getvar "CMDDIA")
        oldOsm (getvar "OSMODE")
  )

  (setvar "BACKGROUNDPLOT" 0)
  (setvar "CMDECHO" 0)
  (setvar "CMDDIA" 0)
  (setvar "OSMODE" 0)

  (princ "\nChon cac doi tuong can in PDF: ")
  (setq ss (ssget))

  (if ss
    (progn
      (setq mayin   "HcPdf Writer"
            khogiay "psk:ISOA3"
            ctb     "monochrome.ctb"
            i       0
            n       (sslength ss)
      )

      (while (< i n)
        (setq ent    (ssname ss i)
              vlaObj (vlax-ename->vla-object ent)
        )
        (vla-GetBoundingBox vlaObj 'minPt 'maxPt)
        (setq p1 (vlax-safearray->list minPt)
              p2 (vlax-safearray->list maxPt)
        )

        (grdraw p1 p2 1)
        (plotWindow mayin khogiay ctb p1 p2)
        (setq i (1+ i))
      )

      (redraw)
    )
    (princ "\nChua chon doi tuong nao.")
  )

  ;; Khoi phuc bien he thong
  (setvar "BACKGROUNDPLOT" oldBg)
  (setvar "CMDECHO" oldCmd)
  (setvar "CMDDIA" oldDia)
  (setvar "OSMODE" oldOsm)
  (princ)
)