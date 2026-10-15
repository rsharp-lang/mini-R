[@info "argument x is required"]
let x = ?"--x" || stop("missing required argument --x");
[@info "argument y is optional, default to 'abcd'"]
let y = ?"--y" || "abcd";
[@info "argument z is optional, is a logical value with default to FALSE"]
let z as boolean = as.logical(?"--z" || FALSE);

print(c(x,y, z));