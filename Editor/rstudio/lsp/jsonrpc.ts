namespace lsp {

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
    export type LSPAny = LSPObject | LSPArray | string | integer | uinteger |
        decimal | boolean | null;
    /**
     * LSP object definition.
     *
     * @since 3.17.0
     */
    export type LSPObject = { [key: string]: LSPAny };
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
}