/// <reference path="vscode/monaco.d.ts" />
/// <reference path="linq.d.ts" />
declare module rstudio {
    function getCodeText(): string;
    function create(): void;
    function setup(): void;
}
declare namespace lsp {
    type integer = number;
    export namespace ErrorCodes {
        const ParseError: integer;
        const InvalidRequest: integer;
        const MethodNotFound: integer;
        const InvalidParams: integer;
        const InternalError: integer;
        /**
         * This is the start range of JSON-RPC reserved error codes.
         * It doesn't denote a real error code. No LSP error codes should
         * be defined between the start and end range. For backwards
         * compatibility the `ServerNotInitialized` and the `UnknownErrorCode`
         * are left in the range.
         *
         * @since 3.16.0
         */
        const jsonrpcReservedErrorRangeStart: integer;
        /** @deprecated use jsonrpcReservedErrorRangeStart */
        const serverErrorStart: integer;
        /**
         * Error code indicating that a server received a notification or
         * request before the server has received the `initialize` request.
         */
        const ServerNotInitialized: integer;
        const UnknownErrorCode: integer;
        /**
         * This is the end range of JSON-RPC reserved error codes.
         * It doesn't denote a real error code.
         *
         * @since 3.16.0
         */
        const jsonrpcReservedErrorRangeEnd = -32000;
        /** @deprecated use jsonrpcReservedErrorRangeEnd */
        const serverErrorEnd: integer;
        /**
         * This is the start range of LSP reserved error codes.
         * It doesn't denote a real error code.
         *
         * @since 3.16.0
         */
        const lspReservedErrorRangeStart: integer;
        /**
         * A request failed but it was syntactically correct, e.g the
         * method name was known and the parameters were valid. The error
         * message should contain human readable information about why
         * the request failed.
         *
         * @since 3.17.0
         */
        const RequestFailed: integer;
        /**
         * The server cancelled the request. This error code should
         * only be used for requests that explicitly support being
         * server cancellable.
         *
         * @since 3.17.0
         */
        const ServerCancelled: integer;
        /**
         * The server detected that the content of a document got
         * modified outside normal conditions. A server should
         * NOT send this error code if it detects a content change
         * in it unprocessed messages. The result even computed
         * on an older state might still be useful for the client.
         *
         * If a client decides that a result is not of any use anymore
         * the client should cancel the request.
         */
        const ContentModified: integer;
        /**
         * The client has canceled a request and a server has detected
         * the cancel.
         */
        const RequestCancelled: integer;
        /**
         * This is the end range of LSP reserved error codes.
         * It doesn't denote a real error code.
         *
         * @since 3.16.0
         */
        const lspReservedErrorRangeEnd: integer;
    }
    export {};
}
declare namespace lsp {
    type integer = number;
    /**
     * Defines an unsigned integer number in the range of 0 to 2^31 - 1.
     */
    export type uinteger = number;
    /**
     * Defines a decimal number. Since decimal numbers are very
     * rare in the language server specification we denote the
     * exact range with every decimal using the mathematics
     * interval notation (e.g. [0, 1] denotes all decimals d with
     * 0 <= d <= 1.
     */
    export type decimal = number;
    /**
     * The LSP any type
     *
     * @since 3.17.0
     */
    export type LSPAny = LSPObject | LSPArray | string | integer | uinteger | decimal | boolean | null;
    /**
     * LSP object definition.
     *
     * @since 3.17.0
     */
    export type LSPObject = {
        [key: string]: LSPAny;
    };
    /**
     * LSP arrays.
     *
     * @since 3.17.0
     */
    export type LSPArray = LSPAny[];
    export interface Message {
        jsonrpc: string;
    }
    export interface RequestMessage extends Message {
        /**
         * The request id.
         */
        id: integer | string;
        /**
         * The method to be invoked.
         */
        method: string;
        /**
         * The method's params.
         */
        params?: LSPArray | object;
    }
    export interface ResponseMessage extends Message {
        /**
         * The request id.
         */
        id: integer | string | null;
        /**
         * The result of a request. This member is REQUIRED on success.
         * This member MUST NOT exist if there was an error invoking the method.
         */
        result?: LSPAny;
        /**
         * The error object in case a request fails.
         */
        error?: ResponseError;
    }
    export interface ResponseError {
        /**
         * A number indicating the error type that occurred.
         */
        code: integer;
        /**
         * A string providing a short description of the error.
         */
        message: string;
        /**
         * A primitive or structured value that contains additional
         * information about the error. Can be omitted.
         */
        data?: LSPAny;
    }
    export interface NotificationMessage extends Message {
        /**
         * The method to be invoked.
         */
        method: string;
        /**
         * The notification's params.
         */
        params?: LSPArray | object;
    }
    export {};
}
declare namespace lsp {
    interface Position {
        /**
         * Line position in a document (zero-based).
         */
        line: uinteger;
        /**
         * Character offset on a line in a document (zero-based). The meaning of this
         * offset is determined by the negotiated `PositionEncodingKind`.
         *
         * If the character value is greater than the line length it defaults back
         * to the line length.
         */
        character: uinteger;
    }
    let host: string;
    /**
     * get symbol information
     *
     * @param document the key that reference to the specific script document inside the server memory
     * @param offset the position offset on the script document
     * @param symbol the symbol name for get the information
    */
    function get_symbol_info(document: string, offset: Position, symbol: string): Promise<void>;
    /**
     * put script text into server memory
     *
     * @param key a hash key that could be used for make reference of this script text
    */
    function put_script(script_str: string, key: string): void;
    /**
     * commit the script document to the filesystem
     *
     * @param key a reference key that associated with a specific script text inside the server memory
     * @param path the local file path for save the script file, must be a local full path
    */
    function commit(key: string, path: string): void;
}
declare namespace lsp {
}
declare namespace rstudio.intellisense {
    function create_intellisense(model: monaco.editor.ITextModel, position: monaco.Position): any;
    const r_keywords: string[];
}
declare module rstudio.tooltip {
    function create_tooltip(model: monaco.editor.ITextModel, position: monaco.Position): any;
    function contentHtml(word: string): string;
    const imports_keyword: string;
    const return_keyword: string;
    const list_keyword: string;
    const logical_keyword: string;
    const keywords: {
        imports: string;
        return: string;
        list: string;
        TRUE: string;
        FALSE: string;
    };
}
