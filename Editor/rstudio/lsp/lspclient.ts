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

    export let host: string = "";

    function url(api: string, key: string) {
        return `${host}${api}/?key=${key}`;
    }

    /**
     * get symbol information
     * 
     * @param document the key that reference to the specific script document inside the server memory
     * @param offset the position offset on the script document
     * @param symbol the symbol name for get the information
    */
    export function get_symbol_info(document: string, offset: Position, symbol: string) {
        return fetch(url("/lsp/get/symbol", document) + `&symbol=${symbol}`).then((response) => {
            return response.text();
        });
    }

    export function get_function_symbols(): Promise<string[]> {
        return fetch(url("/lsp/get/functions", "")).then((response) => {
            return response.json();
        });
    }

    /**
     * put script text into server memory
     * 
     * @param key a hash key that could be used for make reference of this script text
    */
    export function put_script(script_str: string, key: string) {
        let data = { doc: script_str };

        $ts.post(url("/lsp/put", key), data, (response: IMsg<ResponseMessage>) => {

        });
    }

    /**
     * commit the script document to the filesystem
     * 
     * @param key a reference key that associated with a specific script text inside the server memory
     * @param path the local file path for save the script file, must be a local full path
    */
    export function commit(key: string, path: string) {
        let data = { file: path };

        $ts.post(url("/lsp/save", key), data, (response: IMsg<ResponseMessage>) => {

        });
    }

    export function get_file(path: string) {
        return fetch(`/lsp/read/?file=${encodeURIComponent(path)}`).then(response => {
            return response.text();
        });
    }
}