module rstudio.tooltip {

    export function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position): any {
        // 获取光标位置的单词
        const word = model.getWordAtPosition(position);

        if (!word) {
            return null;
        } else {
            return resolveTooltip(word, position);
        }
    }

    function resolveTooltip(word: monaco.editor.IWordAtPosition, position: monaco.Position) {
        // 根据单词显示自定义提示
        return contentHtml(word.word).then(str => {
            const htmlContent = {
                supportHtml: true,
                value: str
            };
            const hover = {
                range: new monaco.Range(
                    position.lineNumber,
                    word.startColumn,
                    position.lineNumber,
                    word.endColumn
                ),
                contents: [htmlContent]
            };

            if (!str) {
                return null;
            } else {
                return hover;
            }
        });
    }

    export function contentHtml(word: string): Promise<string> {
        if (word in keywords) {
            return Promise.resolve<string>(keywords[word]);
        } else {
            return lsp.get_symbol_info(rstudio.hashkey(), null, word);
        }
    }

    function tooltip(title: string, text: string) {
        return `<h3>${title}</h3><p>${text}</p>`;
    }

    export const imports_keyword = tooltip(
        'Loading/attaching of the .NET clr package module',
        'similar to the <code>library</code> and <code>require</code> load and attach add-on .NET clr package modules.');

    export const return_keyword = tooltip(
        'Returns the function value to caller',
        `If value is missing, NULL is returned. If it is a single expression, the value of the evaluated expression is returned. 
(The expression is evaluated as soon as return is called, in the evaluation frame of the function and before any on.exit expression is evaluated.)
If the end of a function is reached without calling return, the value of the last evaluated expression is returned.`);

    export const logical_keyword = tooltip(
        "Logical Vectors",
        `Create or test for objects of type 'logical', and the basic logical constants.
TRUE and FALSE are reserved words denoting logical constants in the R language, whereas T and F are global variables whose initial values set to these. 
All four are logical(1) vectors.
Logical vectors are coerced to integer vectors in contexts where a numerical value is required, with TRUE being mapped to 1L, FALSE to 0L and NA to NA_integer_.`);

    export const let_keyword = tooltip(
        "Create new symbol", "Create a new symbol in current environment frame."
    );

    export const const_keyword = tooltip(
        "Create new symbol",
        "Create a new symbol with lock binding(which means the symbol value could not be changed) in current environment frame.");

    export const from_keyword = tooltip(
        "The .NET clr module source",
        `The .NET clr package module imports source assembly name, usually be the assembly file base name. 
        Assembly file name with dll extension suffix or the full file path to the dll assembly file also could be accepted.`);

    export const function_keyword = tooltip(
        "Define a function",
        "Define a function in R# runtime, a function is kind of expression collection with parameter accept and value returns to its caller.");

    export const if_keyword = tooltip(
        "Control Flow",
        `These are the basic control-flow constructs of the R language. They function in much the same way as control statements in any Algol-like language. They are all reserved words.
if returns the value of the expression evaluated, or NULL invisibly if none was (which may happen if there is no else).`);

    export const for_keyword = tooltip(
        "Control Flow",
        `These are the basic control-flow constructs of the R language. They function in much the same way as control statements in any Algol-like language. They are all reserved words.
for, while and repeat return NULL invisibly. for sets var to the last used element of seq, or to NULL if it was of length zero.`);

    export const step_keyword = tooltip(
        "Set the numeric sequence generator steps",
        `This keyword controls the different of the generated numeric sequence, default step is 1 for the numeric sequence, example as: <code>1:5</code>; 
        you could use <code>step</code> keyword for produce a sequence with different 0.5: <code>1:5 step 0.5</code>.`
    );

    export const null_keyword = tooltip(
        "The Null Object",
        `NULL represents the null object in R: it is a reserved word. NULL is often returned by expressions and functions whose value is undefined.
        NULL can be indexed (see Extract) in just about any syntactically legal way: apart from NULL[[]] which is an error, the result is always NULL. 
        Objects with value NULL can be changed by replacement operators and will be coerced to the type of the right-hand side.

NULL is also used as the empty pairlist: see the examples. Because pairlists are often promoted to lists, you may encounter NULL being promoted to an empty list.

Objects with value NULL cannot have attributes as there is only one null object: attempts to assign them are either an error (attr) or promote the object to an empty list with attribute(s) (attributes and structure).`);

    export const require_keyword = tooltip(
        "Loading/Attaching and Listing of Packages",
        `library and require load and attach add-on packages.
    
    library(package) and require(package) both load the namespace of the package with name package and attach it on the search list. require is designed for use inside other functions; it returns FALSE and gives a warning (rather than an error as library() does by default) if the package does not exist. Both functions check and update the list of currently attached packages and do not reload a namespace which is already loaded. (If you want to reload such a package, call detach(unload = TRUE) or unloadNamespace first.) If you want to load a package without attaching it on the search list, see requireNamespace.
    require returns (invisibly) a logical indicating whether the required package is available.
    `);

    export const library_keyword = tooltip(
        "Loading/Attaching and Listing of Packages",
        `library and require load and attach add-on packages.
            library(package) and require(package) both load the namespace of the package with name package and attach it on the search list. require is designed for use inside other functions; it returns FALSE and gives a warning (rather than an error as library() does by default) if the package does not exist. Both functions check and update the list of currently attached packages and do not reload a namespace which is already loaded. (If you want to reload such a package, call detach(unload = TRUE) or unloadNamespace first.) If you want to load a package without attaching it on the search list, see requireNamespace.

To suppress messages during the loading of packages use suppressPackageStartupMessages: this will suppress all messages from R itself but not necessarily all those from package authors.

If library is called with no package or help argument, it lists all available packages in the libraries specified by lib.loc, and returns the corresponding information in an object of class "libraryIQR". (The structure of this class may change in future versions.) Use .packages(all = TRUE) to obtain just the names of all available packages, and installed.packages() for even more information.

library(help = somename) computes basic information about the package somename, and returns this in an object of class "packageInfo". (The structure of this class may change in future versions.) When used with the default value (NULL) for lib.loc, the attached packages are searched before the libraries.
Normally library returns (invisibly) the list of attached packages, but TRUE or FALSE if logical.return is TRUE. When called as library() it returns an object of class "libraryIQR", and for library(help=), one of class "packageInfo".
            `
    );

    export const keywords = {
        "imports": imports_keyword,
        'return': return_keyword,
        'TRUE': logical_keyword,
        'FALSE': logical_keyword,
        "let": let_keyword,
        "const": const_keyword,
        "from": from_keyword,
        "function": function_keyword,
        "if": if_keyword,
        "for": for_keyword,
        "step": step_keyword,
        "NULL": null_keyword,
        "require": require_keyword,
        "library": library_keyword
    };
}