function highlightText(color) {
    var range, sel;
    if (window.getSelection) {
        // 获取用户选中的文本范围
        sel = window.getSelection();
        if (sel.rangeCount) {
            range = sel.getRangeAt(0);
            // 创建一个新的span元素，并设置颜色
            var span = document.createElement('span');
            span.style.color = color;
            span.textContent = range.toString();
            // 删除选中的文本并插入新的span元素
            range.deleteContents();
            range.insertNode(span);
        }
    } else if (document.selection && document.selection.createRange) {
        range = document.selection.createRange();
        range.execCommand('ForeColor', false, color);
    }
}

// 如果需要处理占位符，可以添加以下代码
document.getElementById('editor').addEventListener('focus', function () {
    if (this.innerHTML === "在此输入文本...") {
        this.innerHTML = '';
    }
});

document.getElementById('editor').addEventListener('blur', function () {
    if (this.innerHTML === '') {
        this.innerHTML = "在此输入文本...";
    }
});



function updateLineNumbers() {
    var editor = document.getElementById('editor');
    var linenums = document.getElementById('linenums');
    var lines = editor.innerHTML.split(/<br\s*\/?>/gi);
    var lineNumbersHTML = '';

    // 为每一行创建行号
    for (var i = 0; i < lines.length; i++) {
        lineNumbersHTML += '<div class="linenumber">' + (i + 1) + '</div>';
    }

    // 更新行号显示
    linenums.innerHTML = lineNumbersHTML;
}

// 当文档加载完毕时，初始化行号
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById("editor").addEventListener('keypress', function() {
        updateLineNumbers()
    });
    updateLineNumbers();
});

