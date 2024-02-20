variable seed
create board 64 chars allot
create stack 64 chars allot
variable n-check
variable game-state

variable gen-idx

variable count-r
variable count-c
variable count-rc
variable count-cc
variable count-cnt

variable reveal-idx

variable show-idx
variable show-data

variable mark-idx
variable mark-data

variable check-sp
variable check-r
variable check-c
variable check-idx
variable check-data

variable check-surrounding-r
variable check-surrounding-c
variable check-surrounding-rc
variable check-surrounding-cc
variable check-surrounding-idx
variable check-surrounding-data

create in-buf 80 chars allot

: msk-mine $80 ;
: msk-check $40 ;
: msk-mark $20 ;
: msk-cnt $f ;

: state-on-going 0 ;
: state-win 1 ;
: state-loss 2 ;

: rand
    seed @
    dup 13 lshift xor
    dup 17 rshift xor
    dup 5 lshift xor
    seed !
    seed @ 63 and
;

: is-valid
    dup -1 >
    swap 8 <
    and
;

: inc dup @ 1+ swap ! ;
: dec dup @ 1- swap ! ;

: count-mine
    count-c !
    count-r !
    0 count-cnt !
    2 -1 do
        2 -1 do
            count-r @ j + count-rc !
            count-c @ i + count-cc !
            count-rc @ is-valid
            count-cc @ is-valid and if
                board count-rc @ 3 lshift count-cc @ + + c@ msk-mine and if
                    count-cnt inc
                then
            then
        loop
    loop
    count-cnt @
;

: gen
    board 64 0 fill
    0 n-check !
    state-on-going game-state !
    10 0 do
        rand gen-idx !
        begin
            board gen-idx @ + c@
        while
            rand gen-idx !
        repeat
        msk-mine board gen-idx @ + c!
    loop
    0 gen-idx !
    8 0 do
        8 0 do
            board gen-idx @ + c@ msk-mine and 0= if
                j i count-mine board gen-idx @ + c!
            then
            gen-idx inc
        loop
    loop
;

: reveal-board
    0 reveal-idx !
    32 32 emit emit
    8 0 do
        i .
    loop
    cr
    8 0 do
        i .
        8 0 do
            board reveal-idx @ + c@ msk-mine and if
                ." * "
            else
                board reveal-idx @ + c@ msk-cnt and .
            then
            reveal-idx inc
        loop
        cr
    loop
;

: show-board
    0 show-idx !
    32 32 emit emit
    8 0 do
        i .
    loop
    cr
    8 0 do
        i .
        8 0 do
            board show-idx @ + c@ show-data !
            show-data @ msk-check and if
                show-data @ msk-mine and if
                    ." * "
                else
                    show-data @ msk-cnt and .
                then
            else
                show-data @ msk-mark and if
                    ." F "
                else
                    ." _ "
                then
            then
            show-idx inc
        loop
        cr
    loop
;

: mark-cell
    swap 3 lshift + mark-idx !
    board mark-idx @ + c@ mark-data !
    mark-data @ msk-check and 0= if
        mark-data @ msk-mark xor board mark-idx @ + c!
    then
;

: check-push-idx
    stack check-sp @ + !
    check-sp inc
;

: check-pop-idx
    check-sp dec
    stack check-sp @ + c@
;

: check-surrounding
    check-surrounding-c !
    check-surrounding-r !
    2 -1 do
        check-surrounding-r @ i + check-surrounding-rc !
        check-surrounding-rc @ is-valid if
            2 -1 do
                check-surrounding-c @ i + check-surrounding-cc !
                check-surrounding-cc @ is-valid if
                    check-surrounding-rc @ 3 lshift check-surrounding-cc @ +
                        check-surrounding-idx !
                    board check-surrounding-idx @ + c@
                        check-surrounding-data !
                    check-surrounding-data @ msk-check msk-mark or and 0= if
                        check-surrounding-data @ msk-check or
                            board check-surrounding-idx @ + c!
                        check-surrounding-idx @ check-push-idx
                        n-check inc
                    then
                then
            loop
        then
    loop
;

: check-cell
    check-c !
    check-r !
    check-r @ 3 lshift check-c @ + check-idx !
    board check-idx @ + c@ check-data !
    check-data @ msk-check msk-mark or and if
        exit
    then
    check-data @ msk-mine and if
        state-loss game-state !
        exit
    then

    0 check-sp !
    check-data @ msk-check or board check-idx @ + c!
    n-check inc
    check-idx @ check-push-idx
    begin
        check-sp @
    while
        check-pop-idx check-idx !
        check-idx @ 7 and check-c !
        check-idx @ 3 rshift check-r !
        board check-idx @ + c@ check-data !

        check-data @ msk-cnt and 0= if
            check-r @ check-c @ check-surrounding
        then
    repeat
    n-check @ 54 = if
        state-win game-state !
    then
;

: c check-cell ;
: m mark-cell ;

: srand
    ." seed: "
    in-buf 80 accept
    cr
    in-buf swap evaluate
    seed !
;

: setup
    srand
    gen
;

: main-loop
    cr
    setup
    show-board
    ." > "
    in-buf 80 accept
    cr
    in-buf swap evaluate
    begin
        game-state @ 0=
    while
        show-board
        ." > "
        in-buf 80 accept
        cr
        in-buf swap evaluate
    repeat
    game-state @ state-win = if
        ." Win" cr
    else
        ." Loss" cr
    then
    reveal-board
;

