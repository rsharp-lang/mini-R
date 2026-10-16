/* ============================================================
   R# Script CVS - frontend logic
   message bridge + three column browser + ansi escape renderer
   ============================================================ */

(function () {
    'use strict';

    /* ============================ protocol ============================ */

    const Outgoing = {
        Ready: 'Ready',
        NewScript: 'NewScript',
        SelectScript: 'SelectScript',
        SelectVersion: 'SelectVersion',
        OpenInEditor: 'OpenInEditor',
        SaveVersion: 'SaveVersion',
        RunScript: 'RunScript',
        DeleteScript: 'DeleteScript',
        RequestOutput: 'RequestOutput'
    };

    const Incoming = {
        Database: 'Database',
        Scripts: 'Scripts',
        Versions: 'Versions',
        Runs: 'Runs',
        VersionDetail: 'VersionDetail',
        Output: 'Output',
        RunResult: 'RunResult',
        Status: 'Status'
    };

    /* ============================ state ============================ */

    const state = {
        scripts: [],
        search: null,        // not null: the script list displays the search hits
        versions: [],
        runs: [],
        scriptId: null,
        versionId: null,
        runId: null,
        metadata: null,
        run: null,         // the currently selected run history record
        args: {},          // name -> value of the parameter form
        formSpec: [],      // the ArgumentSpec list that the form was built from
        formControls: {},  // name -> {kind,node,spec} of the rendered inputs
        stream: 'stdout',  // the visible tab of the output panel
        output: { stdout: '', stderr: '' },
        busy: false
    };

    const MAX_OUTPUT = 300000;

    /* ============================ dom ============================ */

    const dom = {};
    const ids = ['dbPath', 'dbStat', 'statusPill', 'scriptList', 'scriptEmpty',
        'scriptFilter', 'btnNewScript', 'versionList', 'versionEmpty',
        'versionColTitle', 'versionComment', 'btnSaveVersion', 'btnOpenEditor',
        'metaGrid', 'metaDesc', 'metaDeps', 'versionBadge', 'paramForm', 'formCard',
        'runsCard',
        'formHint', 'workdir', 'btnRun', 'btnResetArgs', 'runList', 'runEmpty',
        'btnCopyCmd', 'runsHint', 'outputView', 'outputHint', 'btnCopyOutput',
        'newScriptModal', 'newScriptName', 'btnCreateScript', 'btnCancelNew'];

    ids.forEach(function (id) { dom[id] = document.getElementById(id); });

    /* ============================ bridge ============================ */

    function send(type, payload) {
        window.chrome.webview.postMessage({ type: type, payload: payload === undefined ? null : payload });
    }

    function el(tag, className, text) {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null) node.textContent = text;
        return node;
    }

    function setStatus(text, level) {
        dom.statusPill.textContent = text || 'ready';
        dom.statusPill.setAttribute('data-level', level || 'info');
    }

    function setBusy(busy, text) {
        state.busy = busy;
        dom.btnRun.disabled = busy;
        dom.btnSaveVersion.disabled = busy;
        dom.btnRun.textContent = busy ? '运行中…' : '运行';
        if (busy) setStatus(text || 'running', 'busy');
    }

    /* ============================ ansi ============================ */

    const ANSI_FG = ['black', 'red', 'green', 'yellow', 'blue', 'magenta', 'cyan', 'white'];
    const ANSI_BRIGHT = ['brightBlack', 'brightRed', 'brightGreen', 'brightYellow',
        'brightBlue', 'brightMagenta', 'brightCyan', 'brightWhite'];

    // the vscode light+ terminal palette of the 16 system colors
    const ANSI_16 = [
        '#000000', '#CD3131', '#107C10', '#BF8803', '#0451A5', '#BC05BC', '#0598BC', '#555555',
        '#666666', '#CD3131', '#14CE14', '#B5BA00', '#0451A5', '#BC05BC', '#0598BC', '#A5A5A5'
    ];

    function xterm256(idx) {
        if (idx < 16) return ANSI_16[idx];
        if (idx < 232) {
            const n = idx - 16;
            const r = Math.floor(n / 36), g = Math.floor((n % 36) / 6), b = n % 6;
            const map = [0, 95, 135, 175, 215, 255];
            return '#' + [map[r], map[g], map[b]].map(function (v) {
                return ('0' + v.toString(16)).slice(-2);
            }).join('');
        }
        const v = 8 + (idx - 232) * 10;
        const hex = ('0' + v.toString(16)).slice(-2);
        return '#' + hex + hex + hex;
    }

    function escapeHtml(text) {
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function newAnsiState() {
        return {
            fg: null, bg: null, fgStyle: null, bgStyle: null,
            bold: false, dim: false, italic: false, underline: false, strike: false
        };
    }

    function applySgr(st, codes) {
        const list = String(codes || '0').split(';');
        let i = 0;

        while (i < list.length) {
            const code = parseInt(list[i], 10) || 0;

            if (code === 0) {
                st = newAnsiState();
            } else if (code === 1) { st.bold = true; }
            else if (code === 2) { st.dim = true; }
            else if (code === 3) { st.italic = true; }
            else if (code === 4) { st.underline = true; }
            else if (code === 9) { st.strike = true; }
            else if (code === 22) { st.bold = false; st.dim = false; }
            else if (code === 23) { st.italic = false; }
            else if (code === 24) { st.underline = false; }
            else if (code === 29) { st.strike = false; }
            else if (code === 39) { st.fg = null; st.fgStyle = null; }
            else if (code === 49) { st.bg = null; st.bgStyle = null; }
            else if (code >= 30 && code <= 37) { st.fg = ANSI_FG[code - 30]; st.fgStyle = null; }
            else if (code >= 90 && code <= 97) { st.fg = ANSI_BRIGHT[code - 90]; st.fgStyle = null; }
            else if (code >= 40 && code <= 47) { st.bg = ANSI_FG[code - 40]; st.bgStyle = null; }
            else if (code >= 100 && code <= 107) { st.bg = ANSI_BRIGHT[code - 100]; st.bgStyle = null; }
            else if (code === 38 || code === 48) {
                const isFg = code === 38;
                const mode = parseInt(list[i + 1], 10);

                if (mode === 5) {
                    const color = xterm256(parseInt(list[i + 2], 10) || 0);
                    if (isFg) { st.fg = null; st.fgStyle = color; }
                    else { st.bg = null; st.bgStyle = color; }
                    i += 2;
                } else if (mode === 2) {
                    const rgb = 'rgb(' + (parseInt(list[i + 2], 10) || 0) + ',' +
                        (parseInt(list[i + 3], 10) || 0) + ',' +
                        (parseInt(list[i + 4], 10) || 0) + ')';
                    if (isFg) { st.fg = null; st.fgStyle = rgb; }
                    else { st.bg = null; st.bgStyle = rgb; }
                    i += 4;
                }
            }

            i += 1;
        }

        return st;
    }

    function stateClasses(st) {
        const cls = [];
        if (st.fg) cls.push('fg-' + st.fg);
        if (st.bg) cls.push('bg-' + st.bg);
        if (st.bold) cls.push('ansi-bold');
        if (st.dim) cls.push('ansi-dim');
        if (st.italic) cls.push('ansi-italic');
        if (st.underline) cls.push('ansi-underline');
        if (st.strike) cls.push('ansi-strike');
        return cls;
    }

    function stateStyle(st) {
        const css = [];
        if (st.fgStyle) css.push('color:' + st.fgStyle);
        if (st.bgStyle) css.push('background:' + st.bgStyle);
        return css.length ? css.join(';') : null;
    }

    /**
     * render a text that may contains the ansi escape sequences as html
     * (the sgr sequences are converted into css classes / inline styles)
     */
    function ansiToHtml(text) {
        if (text === null || text === undefined) return '';

        let src = String(text)
            .replace(/\r\n/g, '\n')
            // osc / dcs / charset selection / any other csi but the SGR one
            .replace(/\x1b\][^\x07\x1b]*(?:\x07|\x1b\\)/g, '')
            .replace(/\x1b[PX^_][^\x1b]*(?:\x1b\\)?/g, '')
            .replace(/\x1b[()#][0-9A-Za-z]/g, '')
            .replace(/\x1b\[[0-9;?]*[a-ln-zA-Z]/g, '')
            .replace(/\x1b[=>]/g, '');

        const re = /\x1b\[([0-9;:]*)m/g;
        const chunks = [];
        let st = newAnsiState();
        let last = 0;
        let m;

        while ((m = re.exec(src)) !== null) {
            if (m.index > last) {
                chunks.push({ text: src.slice(last, m.index), state: st });
            }
            last = m.index + m[0].length;
            st = applySgr(st, m[1]);
        }

        if (last < src.length) {
            chunks.push({ text: src.slice(last), state: st });
        }

        return chunks.map(function (c) {
            const cls = stateClasses(c.state);
            const style = stateStyle(c.state);
            const body = escapeHtml(c.text);

            if (!cls.length && !style) return body;

            return '<span class="' + cls.join(' ') + '"' +
                (style ? ' style="' + style + '"' : '') + '>' + body + '</span>';
        }).join('');
    }

    /* ============================ render: scripts ============================ */

    function renderScripts() {
        if (state.search) {
            renderSearchHits();
            return;
        }

        const list = state.scripts;

        dom.scriptList.innerHTML = '';
        dom.scriptEmpty.hidden = list.length > 0;
        dom.scriptEmpty.textContent = '暂无脚本，点击 ＋ 新建';

        list.forEach(function (s) {
            const li = el('li');
            if (s.id === state.scriptId) li.className = 'selected';

            const main = el('div', 'item-main');
            main.appendChild(el('span', 'item-name', s.name || s.id));
            main.appendChild(el('span', 'item-right', s.versionCount || 0));
            li.appendChild(main);
            li.appendChild(el('div', 'item-sub',
                (s.title || 'no title') + ' · ' + (s.updated || '')));

            li.addEventListener('click', function () { selectScript(s.id); });
            dom.scriptList.appendChild(li);
        });
    }

    /**
     * the script list is driven by the qgram full text index of the host:
     * every hit is a script with its similarity rank and a code snippet
     */
    function renderSearchHits() {
        const hits = state.search || [];

        dom.scriptList.innerHTML = '';
        dom.scriptEmpty.hidden = hits.length > 0;
        dom.scriptEmpty.textContent = '没有匹配的脚本';

        hits.forEach(function (hit) {
            const s = hit.script || {};
            const li = el('li');

            if (s.id === state.scriptId) li.className = 'selected';

            const main = el('div', 'item-main');

            main.appendChild(el('span', 'item-name', s.name || s.id));
            main.appendChild(el('span', 'badge', (hit.score || 0).toFixed(2)));
            li.appendChild(main);

            if (hit.snippet) li.appendChild(el('div', 'item-snippet', hit.snippet));
            if (s.title) li.appendChild(el('div', 'item-sub', s.title));

            li.addEventListener('click', function () { selectScript(s.id); });
            dom.scriptList.appendChild(li);
        });
    }

    function selectScript(scriptId) {
        state.scriptId = scriptId;
        state.versionId = null;
        state.runs = [];
        state.metadata = null;
        state.output = { stdout: '', stderr: '' };
        renderScripts();
        renderVersions();
        renderRuns();
        renderMeta(null);
        renderForm([]);
        setOutput('');
        send(Outgoing.SelectScript, { scriptId: scriptId });
    }

    /* ============================ render: versions ============================ */

    function renderVersions() {
        const script = findScript(state.scriptId);

        dom.versionList.innerHTML = '';
        dom.versionColTitle.textContent = script ? (script.name || script.id) : '历史版本';
        dom.versionEmpty.hidden = state.versions.length > 0;
        dom.versionEmpty.textContent = script ? '该脚本尚无版本' : '请先选择一个脚本';

        // the newest version comes first
        state.versions.slice().reverse().forEach(function (v) {
            const li = el('li');
            if (v.versionId === state.versionId) li.className = 'selected';

            const main = el('div', 'item-main');
            main.appendChild(el('span', 'item-name', v.versionId));

            if (script && v.versionId === script.latestVersionId) {
                main.appendChild(el('span', 'badge current', '最新'));
            }
            main.appendChild(el('span', 'item-right', v.created || ''));
            li.appendChild(main);
            li.appendChild(el('div', 'item-sub', v.comment || '(无变更说明)'));

            li.addEventListener('click', function () { selectVersion(v.versionId); });
            li.addEventListener('dblclick', function () { openInEditor(v.versionId); });
            dom.versionList.appendChild(li);
        });
    }

    function selectVersion(versionId) {
        state.versionId = versionId;
        state.output = { stdout: '', stderr: '' };
        renderVersions();
        setOutput('');
        send(Outgoing.SelectVersion, { scriptId: state.scriptId, versionId: versionId });
    }

    function openInEditor(versionId) {
        if (!versionId) return;
        send(Outgoing.OpenInEditor, { scriptId: state.scriptId, versionId: versionId });
        setStatus('open in editor', 'info');
    }

    /* ============================ render: metadata ============================ */

    function renderMeta(meta) {
        state.metadata = meta;
        dom.metaGrid.innerHTML = '';
        dom.metaDeps.innerHTML = '';
        dom.metaDesc.textContent = '';

        if (!meta) {
            dom.versionBadge.textContent = '';
            return;
        }

        dom.versionBadge.textContent = state.versionId || '';

        const rows = [
            ['标题', meta.title],
            ['作者', (meta.authors || []).join(', ')],
            ['参数', (meta.arguments || []).length + ' 个'],
            ['脚本长度', '']
        ];

        rows.forEach(function (row) {
            if (row[1] === undefined) return;
            dom.metaGrid.appendChild(el('div', 'meta-key', row[0]));
            dom.metaGrid.appendChild(el('div', 'meta-val', String(row[1])));
        });

        if (meta.description) {
            dom.metaDesc.textContent = meta.description;
        }
        if (meta.parseError) {
            const tag = el('span', 'tag warn', 'R# parse: ' + meta.parseError);
            dom.metaDeps.appendChild(tag);
        }
        (meta.dependencies || []).forEach(function (dep) {
            dom.metaDeps.appendChild(el('span', 'tag', dep));
        });
    }

    /* ============================ render: parameter form ============================ */

    /**
     * the hint of the parameter form card: the argument count plus an
     * optional note (eg. the run history record that the values come from)
     */
    function updateFormHint(note) {
        const n = (state.formSpec || []).length;
        let text = n ? n + ' 个参数' : '该脚本没有声明命令行参数';

        if (note) text += ' · ' + note;
        dom.formHint.textContent = text;
    }

    function specDefault(spec) {
        if (!spec || spec.defaultValue === null || spec.defaultValue === undefined) return '';
        return String(spec.defaultValue);
    }

    function controlValue(ctl) {
        if (ctl.kind === 'boolean') return ctl.node.checked ? 'TRUE' : 'FALSE';
        return ctl.node.value;
    }

    function setControlValue(ctl, value) {
        if (ctl.kind === 'boolean') {
            const text = (value === null || value === undefined ? '' : String(value)).trim();

            ctl.node.checked = /^(true|1|yes|on)$/i.test(text);
        } else {
            ctl.node.value = value === null || value === undefined ? '' : String(value);
        }
    }

    function renderForm(args) {
        dom.paramForm.innerHTML = '';
        state.args = {};
        state.formSpec = args || [];
        state.formControls = {};

        updateFormHint();

        if (!args || !args.length) {
            return;
        }

        args.forEach(function (arg) {
            const field = el('div', 'field');
            const label = el('span', 'field-label');
            const isBool = String(arg.type || '').toLowerCase() === 'boolean';

            label.appendChild(el('span', null, arg.name));
            label.appendChild(el('span', 'arg-type', arg.type || 'string'));
            if (arg.required) label.appendChild(el('span', 'req', '必填'));

            field.appendChild(label);

            let ctl;

            if (isBool) {
                const wrap = el('label', 'switch');
                const box = document.createElement('input');

                box.type = 'checkbox';
                box.checked = String(arg.defaultValue).toUpperCase() === 'TRUE';
                box.addEventListener('change', function () { state.args[arg.name] = box.checked ? 'TRUE' : 'FALSE'; });

                wrap.appendChild(box);
                wrap.appendChild(el('span', null, '启用'));
                field.appendChild(wrap);

                ctl = { kind: 'boolean', node: box, spec: arg };
            } else {
                const input = document.createElement('input');

                input.type = arg.isNumeric ? 'number' : 'text';
                input.value = arg.defaultValue === null || arg.defaultValue === undefined ? '' : arg.defaultValue;
                input.placeholder = arg.required ? 'required' : '(default: ' + (arg.defaultValue || '""') + ')';
                input.addEventListener('input', function () { state.args[arg.name] = input.value; });

                field.appendChild(input);

                ctl = { kind: 'value', node: input, spec: arg };
            }

            // remember the input node so that a run history record can push
            // its argument values back into the form later
            state.formControls[arg.name] = ctl;
            state.args[arg.name] = controlValue(ctl);

            if (arg.description) {
                field.appendChild(el('div', 'arg-desc', arg.description));
            }

            dom.paramForm.appendChild(field);
        });
    }

    /* ============================ render: runs ============================ */

    function renderRuns() {
        dom.runList.innerHTML = '';
        dom.runEmpty.hidden = state.runs.length > 0;
        dom.runsHint.textContent = state.runs.length
            ? state.runs.length + ' 次运行 · 点击回填参数'
            : '';

        state.runs.slice().reverse().forEach(function (run) {
            const li = el('li');
            if (run.runId === state.runId) li.className = 'selected';

            const main = el('div', 'item-main run-item');
            main.appendChild(el('span', 'item-name', '#' + run.runId));
            main.appendChild(el('span', run.exitCode === 0 ? 'badge ok' : 'badge err',
                'exit ' + run.exitCode));
            main.appendChild(el('span', 'item-right', (run.durationMs || 0) + 'ms · ' + (run.time || '')));
            li.appendChild(main);

            const argText = (run.arguments || []).map(function (a) {
                return '--' + a.name + ' ' + a.value;
            }).join(' ');

            li.appendChild(el('div', 'item-sub', (run.workdir || '(default)') + '  ' + argText));
            li.title = run.commandLine
                ? '点击把该次运行的命令行参数回填到运行参数\n' + run.commandLine
                : '点击把该次运行的命令行参数回填到运行参数';

            li.addEventListener('click', function () { selectRun(run); });
            dom.runList.appendChild(li);
        });

        // the floating copy button only makes sense for a selected run that
        // actually carries a recorded command line
        const showFab = !!(state.run && state.run.commandLine);

        dom.btnCopyCmd.hidden = !showFab;
        if (dom.runsCard) dom.runsCard.classList.toggle('has-fab', showFab);
    }

    let flashTimer = null;

    function flashForm() {
        if (!dom.formCard) return;

        dom.formCard.classList.remove('flash');
        // force a reflow, otherwise the animation is not restarted
        void dom.formCard.offsetWidth;
        dom.formCard.classList.add('flash');

        if (flashTimer) clearTimeout(flashTimer);
        flashTimer = setTimeout(function () { dom.formCard.classList.remove('flash'); }, 1000);
    }

    /**
     * push the commandline arguments of one historical run back into the
     * parameter form. the parameters that were removed from the script (they
     * do not exist in the metadata of the current version any more) can not
     * be restored, they are reported via the status pill instead.
     */
    function applyRunArguments(run) {
        if (!run) return;

        const controls = state.formControls;
        const values = {};
        const unknown = [];
        let filled = 0;

        (run.arguments || []).forEach(function (a) {
            if (a && a.name) {
                values[a.name] = a.value === null || a.value === undefined ? '' : String(a.value);
            }
        });

        Object.keys(controls).forEach(function (name) {
            const ctl = controls[name];
            const restore = Object.prototype.hasOwnProperty.call(values, name);

            // a parameter that the record does not carry falls back to the
            // default value, so that the form exactly reproduces that run
            setControlValue(ctl, restore ? values[name] : specDefault(ctl.spec));
            state.args[name] = controlValue(ctl);

            if (restore) filled += 1;
        });

        Object.keys(values).forEach(function (name) {
            if (!controls[name]) unknown.push(name);
        });

        if (run.workdir) {
            dom.workdir.value = run.workdir;
        }

        updateFormHint('回填自 #' + run.runId);
        flashForm();

        if (unknown.length) {
            setStatus('回填 ' + filled + ' 个参数，' + unknown.length +
                ' 个已不存在: ' + unknown.join(', '), 'warn');
        } else {
            setStatus('已回填 #' + run.runId + ' 的 ' + filled + ' 个参数', 'ok');
        }
    }

    function selectRun(run) {
        state.runId = run.runId;
        state.run = run;
        renderRuns();
        applyRunArguments(run);
        send(Outgoing.RequestOutput, { scriptId: state.scriptId, runId: run.runId });
    }

    /* ============================ render: output ============================ */

    function setOutput(text) {
        const raw = text === null || text === undefined ? '' : String(text);
        const truncated = raw.length > MAX_OUTPUT;
        const body = truncated ? raw.slice(0, MAX_OUTPUT) : raw;

        dom.outputView.innerHTML = ansiToHtml(body);
        dom.outputHint.textContent = truncated
            ? '已截断显示（共 ' + raw.length + ' 字符）'
            : (raw.length ? raw.length + ' 字符' : '');

        if (truncated) {
            const note = el('div', null, '\n… output truncated, use the copy button for the full text …');
            dom.outputView.appendChild(note);
        }
    }

    function refreshOutput() {
        setOutput(state.output[state.stream] || '');
    }

    /* ============================ host messages ============================ */

    const handlers = {};

    handlers[Incoming.Database] = function (payload) {
        dom.dbPath.textContent = payload && payload.path ? payload.path : '';
        if (payload && payload.statistic) {
            const s = payload.statistic;
            dom.dbStat.textContent = s.scripts + ' scripts / ' + s.versions + ' versions / ' + s.runs + ' runs';
        }
    };

    handlers[Incoming.Scripts] = function (payload) {
        state.scripts = payload || [];
        // a full script list reply always leaves the search mode
        state.search = null;

        if (state.scripts.length && !state.scriptId) {
            state.scriptId = state.scripts[0].id;
        }
        renderScripts();
    };

    handlers[Incoming.SearchResults] = function (payload) {
        state.search = payload || [];
        renderScripts();
    };

    handlers[Incoming.Versions] = function (payload) {
        state.versions = payload || [];
        if (!state.versionId && state.versions.length) {
            state.versionId = state.versions[state.versions.length - 1].versionId;
        }
        renderVersions();
    };

    handlers[Incoming.Runs] = function (payload) {
        state.runs = payload || [];
        renderRuns();
    };

    handlers[Incoming.VersionDetail] = function (payload) {
        if (!payload) return;
        state.versionId = payload.versionId || state.versionId;
        renderMeta(payload.metadata);
        renderForm(payload.metadata ? payload.metadata.arguments : []);
        if (payload.workdir) dom.workdir.value = payload.workdir;
        renderVersions();
    };

    handlers[Incoming.Output] = function (payload) {
        if (!payload) return;
        state.output.stdout = payload.stdout || '';
        state.output.stderr = payload.stderr || '';
        state.runId = payload.runId || state.runId;
        refreshOutput();
        renderRuns();
    };

    handlers[Incoming.RunResult] = function (payload) {
        setBusy(false);
        if (!payload) return;

        if (payload.ok) {
            state.output.stdout = payload.stdout || '';
            state.output.stderr = payload.stderr || '';
            refreshOutput();
            setStatus('exit ' + payload.exitCode, payload.exitCode === 0 ? 'ok' : 'error');
        } else {
            setStatus(payload.message || 'run failed', 'error');
        }
    };

    handlers[Incoming.Status] = function (payload) {
        if (!payload) return;
        setStatus(payload.text, payload.level);
    };

    /* ============================ ui events ============================ */

    // the filter box is a full text search box now: the query is evaluated by
    // the qgram index of the host side (debounced, an empty query lists all)
    let searchTimer = null;

    dom.scriptFilter.addEventListener('input', function () {
        const q = (dom.scriptFilter.value || '').trim();

        if (searchTimer) clearTimeout(searchTimer);

        searchTimer = setTimeout(function () {
            if (!q) {
                state.search = null;
                renderScripts();
            }

            send(Outgoing.SearchScripts, { query: q });
        }, 250);
    });

    dom.btnNewScript.addEventListener('click', function () {
        dom.newScriptName.value = 'script' + (state.scripts.length + 1) + '.R';
        dom.newScriptModal.hidden = false;
        dom.newScriptName.focus();
    });

    dom.btnCancelNew.addEventListener('click', function () { dom.newScriptModal.hidden = true; });

    // click on the mask (or press escape) also closes the dialog
    document.querySelector('.modal-mask').addEventListener('click', function () {
        dom.newScriptModal.hidden = true;
    });

    dom.btnCreateScript.addEventListener('click', function () {
        const name = (dom.newScriptName.value || '').trim();

        dom.newScriptModal.hidden = true;
        if (!name) return;

        send(Outgoing.NewScript, { name: name });
        setStatus('creating script', 'busy');
    });

    dom.newScriptName.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') dom.btnCreateScript.click();
        if (e.key === 'Escape') dom.newScriptModal.hidden = true;
    });

    dom.btnOpenEditor.addEventListener('click', function () { openInEditor(state.versionId); });

    dom.btnSaveVersion.addEventListener('click', function () {
        if (!state.scriptId) {
            setStatus('no script selected', 'warn');
            return;
        }

        send(Outgoing.SaveVersion, {
            scriptId: state.scriptId,
            comment: (dom.versionComment.value || '').trim()
        });
        dom.versionComment.value = '';
        setStatus('saving', 'busy');
    });

    dom.btnRun.addEventListener('click', function () {
        if (!state.versionId) {
            setStatus('no version selected', 'warn');
            return;
        }

        const args = Object.keys(state.args).map(function (name) {
            return { name: name, value: state.args[name] };
        });

        setBusy(true, 'running');

        send(Outgoing.RunScript, {
            scriptId: state.scriptId,
            versionId: state.versionId,
            args: args,
            workdir: (dom.workdir.value || '').trim()
        });
    });

    dom.btnResetArgs.addEventListener('click', function () {
        renderForm(state.metadata ? state.metadata.arguments : []);
    });

    dom.btnCopyOutput.addEventListener('click', function () {
        const text = state.output[state.stream] || '';

        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(text);
            } else {
                const ta = document.createElement('textarea');
                ta.value = text;
                document.body.appendChild(ta);
                ta.select();
                document.execCommand('copy');
                document.body.removeChild(ta);
            }
            setStatus('copied', 'ok');
        } catch (e) {
            setStatus('copy failed', 'warn');
        }
    });

    dom.btnCopyCmd.addEventListener('click', function (e) {
        // do not also toggle the run selection when the copy button is clicked
        e.stopPropagation();
        if (!state.run || !state.run.commandLine) return;

        const text = state.run.commandLine;

        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(text);
            } else {
                const ta = document.createElement('textarea');
                ta.value = text;
                document.body.appendChild(ta);
                ta.select();
                document.execCommand('copy');
                document.body.removeChild(ta);
            }
            setStatus('命令行已复制 #' + state.run.runId, 'ok');
        } catch (err) {
            setStatus('复制失败', 'warn');
        }
    });

    Array.prototype.forEach.call(document.querySelectorAll('.tab'), function (tab) {
        tab.addEventListener('click', function () {
            state.stream = tab.getAttribute('data-stream');
            Array.prototype.forEach.call(document.querySelectorAll('.tab'), function (t) {
                t.className = 'tab' + (t === tab ? ' active' : '');
            });
            refreshOutput();
        });
    });

    /* ============================ bootstrap ============================ */

    window.chrome.webview.addEventListener('message', function (event) {
        const msg = event.data || {};
        const handler = handlers[msg.type];

        if (handler) {
            try {
                handler(msg.payload);
            } catch (e) {
                setStatus('ui error: ' + e.message, 'error');
            }
        }
    });

    function findScript(scriptId) {
        for (let i = 0; i < state.scripts.length; i++) {
            if (state.scripts[i].id === scriptId) return state.scripts[i];
        }
        return null;
    }

    renderScripts();
    renderVersions();
    renderRuns();
    setOutput('');

    // tell the host that the page is ready for receive the database content
    send(Outgoing.Ready);
})();
