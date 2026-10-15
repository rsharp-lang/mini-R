[@desc "argument x is required"]
let x = ?"--x" || stop("missing required argument --x");
let y = ?"--y" || "abcd";
let z as boolean = as.logical(?"--z" || FALSE);

print(c(x,y, z));