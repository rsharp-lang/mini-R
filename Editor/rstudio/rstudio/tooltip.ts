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
        "for": for_keyword
    };
}