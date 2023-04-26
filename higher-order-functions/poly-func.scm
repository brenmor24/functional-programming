;;
;;  Write a function (polynomial coefs) that accepts a list of coefficients,
;;  but instead of evaluating the polynomial immediately it returns a unary
;;  function p(x) that can be used to evaluate the polynomial
;;
;;  You may not use your solutiomn from before. 
;;  Your solution should recursively use `polynomial`. 
;;;;;;;;;


(define (polynomial coefs)
    (cond
        ((null? coefs) (lambda (x) 0))
        (else 
            (lambda (x) 
                (+ 
                    (car coefs) 
                    (* x ((polynomial (cdr coefs)) x))
                )
            )
        )
    )
)

;;;;;;;;;;;;;
;; Tests

(define (println s) (begin (display s) (newline)))

(println 
  ((polynomial `()) 5)  ; Expext 0
  )

(println
  ((polynomial `(10 3 -4 2)) 5)  ; Expect 175
  )


(println
  ((polynomial `(10)) 3) ; Expect 10
  )



