namespace lsp {

    export interface Position {
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

    export function get_symbol_info(document: string, offset: Position, symbol: string) {
        return fetch(`/lsp/get/symbol`).then((response) => {

        });
    }
}