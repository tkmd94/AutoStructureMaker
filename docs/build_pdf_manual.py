# -*- coding: utf-8 -*-
"""
AutoStructureMaker v2.0.3 - Comprehensive Technical & Clinical Manual PDF Generator
Combines all 8 documentation files into a single, beautifully-styled, publication-grade PDF.
"""

import os
import re
import base64
import subprocess
import tempfile
import mistune

MERMAID_JS_PATH = os.path.join(
    os.path.expanduser("~"),
    ".antigravity-ide",
    "extensions",
    "shd101wyy.markdown-preview-enhanced-0.8.34-universal",
    "crossnote",
    "dependencies",
    "mermaid",
    "mermaid.min.js"
)
CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
WORKSPACE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUTPUT_PDF_ROOT = os.path.join(WORKSPACE_DIR, "AutoStructureMaker_Manual.pdf")
OUTPUT_PDF_DOCS = os.path.join(WORKSPACE_DIR, "docs", "AutoStructureMaker_Manual.pdf")
OUTPUT_PDF_RES = os.path.join(WORKSPACE_DIR, "AutoStructureMaker", "Resources", "AutoStructureMaker_Manual.pdf")

CHAPTERS = [
    {
        "id": "chapter-1",
        "num": "第1章",
        "title": "システム概要とクイックスタート",
        "source": "README.md",
        "desc": "AutoStructureMaker の目的、基本機能、UI 構成、およびクイックスタート手順"
    },
    {
        "id": "chapter-2",
        "num": "第2章",
        "title": "操作モジュール仕様・解像度整合",
        "source": "MODULES.md",
        "desc": "Add / Delete / Boolean / Margin / ConvertHighRes の構文、解像度モデル、および ⚡ Auto-Align"
    },
    {
        "id": "chapter-3",
        "num": "第3章",
        "title": "構造化 XML テンプレート・臨床プロトコル集",
        "source": "TEMPLATES.md",
        "desc": "XML テンプレート仕様、レガシー CSV 互換性、および前立腺・肺SBRT・頭頸部・乳房の実践プロトコル"
    },
    {
        "id": "chapter-4",
        "num": "第4章",
        "title": "臨床受入試験（コミッショニング）手順書",
        "source": "COMMISSIONING.md",
        "desc": "AAPM TG-275 / MPPG 5.a 準拠の受入試験項目、幾何精度検証プロトコル、および承認署名票"
    },
    {
        "id": "chapter-5",
        "num": "第5章",
        "title": "トラブルシューティング・FAQ",
        "source": "TROUBLESHOOTING.md",
        "desc": "Eclipse 権限エラー、UNC 遅延対策、Pre-Flight 検査エラー（TargetStructure 検知）、および FAQ"
    },
    {
        "id": "chapter-6",
        "num": "第6章",
        "title": "システムアーキテクチャ・設計仕様書",
        "source": "ARCHITECTURE.md",
        "desc": "MVVM パターン、先行ステップ輪郭伝播、インプレース差分同期、および例外安全設計"
    },
    {
        "id": "chapter-7",
        "num": "第7章",
        "title": "開発者ガイド・コーディング規約",
        "source": "CONTRIBUTING.md",
        "desc": "MSBuild x64 ビルド、単体テスト規約、コレクション同期規約、および PR ガイドライン"
    },
    {
        "id": "appendix",
        "num": "付録",
        "title": "更新履歴 (Changelog)",
        "source": "CHANGELOG.md",
        "desc": "v2.0.3、v2.0.2、v2.0.1、v2.0.0、v1.0.0 のリリースノートおよび全重要変更履歴"
    }
]

def get_base64_image(rel_path):
    full_path = os.path.join(WORKSPACE_DIR, rel_path)
    if os.path.exists(full_path):
        with open(full_path, "rb") as f:
            encoded = base64.b64encode(f.read()).decode("ascii")
        ext = os.path.splitext(full_path)[1].lower().replace(".", "")
        if ext == "jpg": ext = "jpeg"
        return f"data:image/{ext};base64,{encoded}"
    return ""

# Initialize Mistune parser with escape=False to preserve raw HTML (callouts, <br>, etc.)
markdown_parser = mistune.Markdown(escape=False)

def preprocess_markdown(text, chapter_id):
    # 1. Normalize line endings
    text = text.replace("\r\n", "\n")

    # 2. Shields.io badges replacement (specifically in README)
    text = re.sub(r'\[!\[Eclipse[^\]]*\]\([^)]+\)\]\([^)]+\)', '', text)
    text = re.sub(r'\[!\[\.NET[^\]]*\]\([^)]+\)\]\([^)]+\)', '', text)
    text = re.sub(r'\[!\[Tests[^\]]*\]\([^)]+\)\]\([^)]*\)', '', text)
    text = re.sub(r'\[!\[License[^\]]*\]\([^)]+\)\]\([^)]+\)', '', text)

    # 3. Replace relative links to chapters
    link_map = {
        "README.md": "#chapter-1",
        "MODULES.md": "#chapter-2",
        "TEMPLATES.md": "#chapter-3",
        "COMMISSIONING.md": "#chapter-4",
        "TROUBLESHOOTING.md": "#chapter-5",
        "ARCHITECTURE.md": "#chapter-6",
        "CONTRIBUTING.md": "#chapter-7",
        "CHANGELOG.md": "#appendix"
    }
    for doc_name, anchor in link_map.items():
        text = text.replace(f"({doc_name})", f"({anchor})")
        text = text.replace(f"({doc_name}#", f"({anchor}-")

    # 4. Replace image paths with base64 data URIs
    ui_sample_b64 = get_base64_image(r"docs\images\AutoStructureMaker_UI_Sample.png")
    if ui_sample_b64:
        text = text.replace("docs/images/AutoStructureMaker_UI_Sample.png", ui_sample_b64)

    # 5. Process GitHub Callouts / Alerts (> [!NOTE], etc.)
    lines = text.split("\n")
    processed_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        callout_match = re.match(r'^>\s*\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*(.*)$', line, re.IGNORECASE)
        if callout_match:
            alert_type = callout_match.group(1).upper()
            first_text = callout_match.group(2).strip()
            callout_content = []
            if first_text:
                callout_content.append(first_text)
            i += 1
            while i < len(lines) and lines[i].startswith(">"):
                callout_content.append(lines[i][1:].strip())
                i += 1
            inner_md = "\n".join(callout_content)
            inner_html = markdown_parser(inner_md)
            processed_lines.append(f'<div class="callout callout-{alert_type.lower()}"><div class="callout-title">{alert_type}</div><div class="callout-body">{inner_html}</div></div>')
            continue
        else:
            processed_lines.append(line)
            i += 1

    text = "\n".join(processed_lines)
    return text

def build_html_document():
    # Read Mermaid JS
    mermaid_js_content = ""
    if os.path.exists(MERMAID_JS_PATH):
        with open(MERMAID_JS_PATH, "r", encoding="utf-8") as f:
            mermaid_js_content = f.read()

    rendered_chapters_html = []
    for chap in CHAPTERS:
        source_path = os.path.join(WORKSPACE_DIR, chap["source"])
        with open(source_path, "r", encoding="utf-8") as f:
            raw_content = f.read()

        preprocessed = preprocess_markdown(raw_content, chap["id"])
        chap_html = markdown_parser(preprocessed)

        # Convert <pre><code class="lang-mermaid">...</code></pre> to <div class="mermaid">...</div>
        chap_html = re.sub(
            r'<pre><code class="lang-mermaid">(.*?)</code></pre>',
            lambda m: f'<div class="mermaid">\n{re.sub(r"&gt;", ">", re.sub(r"&lt;", "<", re.sub(r"&amp;", "&", m.group(1))))}\n</div>',
            chap_html,
            flags=re.DOTALL
        )

        section_block = f"""
        <section class="chapter-section" id="{chap['id']}">
            <div class="chapter-header">
                <div class="chapter-badge">{chap['num']}</div>
                <h1 class="chapter-main-title">{chap['title']}</h1>
                <div class="chapter-lead">{chap['desc']}</div>
            </div>
            <div class="chapter-content">
                {chap_html}
            </div>
        </section>
        """
        rendered_chapters_html.append(section_block)

    # Build TOC HTML
    toc_items = []
    for chap in CHAPTERS:
        toc_items.append(f"""
        <a href="#{chap['id']}" class="toc-item">
            <div class="toc-badge">{chap['num']}</div>
            <div class="toc-info">
                <div class="toc-title">{chap['title']}</div>
                <div class="toc-desc">{chap['desc']}</div>
            </div>
            <div class="toc-arrow">&rarr;</div>
        </a>
        """)
    toc_html = "\n".join(toc_items)

    html_template = f"""<!DOCTYPE html>
<html lang="ja">
<head>
<meta charset="utf-8">
<title>AutoStructureMaker v2.0.3 総合技術・臨床運用マニュアル</title>
<style>
/* ================= PAGE SETUP & BASE STYLING ================= */
@page {{
    size: A4;
    margin: 18mm 16mm 20mm 16mm;
    @top-left {{
        content: "AutoStructureMaker v2.0.3 総合マニュアル";
        font-family: 'Segoe UI', Meiryo, sans-serif;
        font-size: 8pt;
        color: #64748b;
        border-bottom: 0.5pt solid #cbd5e1;
        padding-bottom: 4px;
    }}
    @top-right {{
        content: "Varian Eclipse ESAPI Plugin";
        font-family: 'Segoe UI', Meiryo, sans-serif;
        font-size: 8pt;
        color: #64748b;
        border-bottom: 0.5pt solid #cbd5e1;
        padding-bottom: 4px;
    }}
    @bottom-right {{
        content: "Page " counter(page);
        font-family: 'Segoe UI', Meiryo, sans-serif;
        font-size: 8pt;
        color: #64748b;
    }}
    @bottom-left {{
        content: "Confidential - Radiation Oncology & Medical Physics";
        font-family: 'Segoe UI', Meiryo, sans-serif;
        font-size: 8pt;
        color: #94a3b8;
    }}
}}

* {{
    box-sizing: border-box;
}}

body {{
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Yu Gothic UI", Meiryo, "Hiragino Sans", sans-serif;
    color: #1e293b;
    background-color: #ffffff;
    line-height: 1.68;
    font-size: 10pt;
    margin: 0;
    padding: 0;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}}

/* ================= COVER PAGE ================= */
.cover-page {{
    page-break-after: always;
    min-height: 92vh;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 20px 10px 40px 10px;
}}

.cover-top {{
    border-top: 6px solid #1e3a8a;
    padding-top: 30px;
}}

.cover-badge-row {{
    display: flex;
    gap: 10px;
    margin-bottom: 25px;
}}

.badge-pill {{
    display: inline-block;
    padding: 4px 12px;
    border-radius: 9999px;
    font-size: 8.5pt;
    font-weight: 600;
    letter-spacing: 0.5px;
}}

.badge-blue {{ background: #eff6ff; color: #1e40af; border: 1px solid #bfdbfe; }}
.badge-green {{ background: #ecfdf5; color: #065f46; border: 1px solid #a7f3d0; }}
.badge-purple {{ background: #faf5ff; color: #6b21a8; border: 1px solid #e9d5ff; }}

.cover-title-group {{
    margin-top: 20px;
    margin-bottom: 20px;
}}

.cover-product {{
    font-size: 32pt;
    font-weight: 800;
    color: #0f172a;
    letter-spacing: -0.5px;
    margin: 0;
    line-height: 1.15;
}}

.cover-version {{
    font-size: 18pt;
    font-weight: 600;
    color: #2563eb;
    margin-top: 5px;
    margin-bottom: 15px;
}}

.cover-subtitle {{
    font-size: 16pt;
    font-weight: 600;
    color: #334155;
    line-height: 1.4;
    margin-bottom: 12px;
}}

.cover-desc {{
    font-size: 11pt;
    color: #64748b;
    max-width: 90%;
    line-height: 1.6;
}}

.cover-spec-grid {{
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
    margin-top: 40px;
}}

.spec-card {{
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 16px 20px;
}}

.spec-label {{
    font-size: 8pt;
    text-transform: uppercase;
    color: #64748b;
    font-weight: 700;
    margin-bottom: 4px;
}}

.spec-val {{
    font-size: 10.5pt;
    font-weight: 600;
    color: #0f172a;
}}

.cover-bottom {{
    border-top: 1px solid #e2e8f0;
    padding-top: 20px;
    display: flex;
    justify-content: space-between;
    font-size: 8.5pt;
    color: #64748b;
}}

/* ================= TABLE OF CONTENTS ================= */
.toc-page {{
    page-break-after: always;
    page-break-before: always;
    padding: 10px 0;
}}

.toc-header {{
    border-bottom: 2px solid #0f172a;
    padding-bottom: 12px;
    margin-bottom: 25px;
}}

.toc-header h2 {{
    font-size: 20pt;
    color: #0f172a;
    margin: 0;
}}

.toc-list {{
    display: flex;
    flex-direction: column;
    gap: 12px;
}}

.toc-item {{
    display: flex;
    align-items: center;
    text-decoration: none;
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 14px 18px;
    color: inherit;
    transition: all 0.2s ease;
}}

.toc-badge {{
    background: #1e3a8a;
    color: #ffffff;
    font-weight: 700;
    font-size: 9pt;
    padding: 6px 12px;
    border-radius: 6px;
    margin-right: 18px;
    min-width: 65px;
    text-align: center;
}}

.toc-info {{
    flex: 1;
}}

.toc-title {{
    font-size: 11pt;
    font-weight: 700;
    color: #0f172a;
    margin-bottom: 3px;
}}

.toc-desc {{
    font-size: 8.5pt;
    color: #64748b;
}}

.toc-arrow {{
    font-size: 14pt;
    color: #94a3b8;
    margin-left: 15px;
}}

/* ================= CHAPTER STYLING ================= */
.chapter-section {{
    page-break-before: always;
    margin-top: 10px;
}}

.chapter-header {{
    background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 100%);
    color: #ffffff;
    padding: 24px 28px;
    border-radius: 10px;
    margin-bottom: 25px;
}}

.chapter-badge {{
    display: inline-block;
    background: rgba(255, 255, 255, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.3);
    padding: 3px 10px;
    border-radius: 4px;
    font-size: 8.5pt;
    font-weight: 700;
    letter-spacing: 1px;
    margin-bottom: 8px;
}}

.chapter-main-title {{
    font-size: 18pt;
    font-weight: 800;
    color: #ffffff;
    margin: 4px 0 8px 0;
}}

.chapter-lead {{
    font-size: 9.5pt;
    color: #cbd5e1;
    line-height: 1.5;
}}

/* ================= CONTENT HEADINGS ================= */
h1, h2, h3, h4, h5, h6 {{
    color: #0f172a;
    font-weight: 700;
    page-break-after: avoid;
    line-height: 1.35;
}}

.chapter-content > h1:first-child {{
    display: none; /* Already presented in chapter-header */
}}

h1 {{
    font-size: 15pt;
    border-bottom: 1.5pt solid #e2e8f0;
    padding-bottom: 6px;
    margin-top: 28px;
    margin-bottom: 14px;
}}

h2 {{
    font-size: 13pt;
    border-left: 4px solid #2563eb;
    padding-left: 10px;
    margin-top: 24px;
    margin-bottom: 12px;
}}

h3 {{
    font-size: 11pt;
    color: #1e293b;
    margin-top: 20px;
    margin-bottom: 8px;
}}

h4 {{
    font-size: 10pt;
    color: #334155;
    margin-top: 14px;
    margin-bottom: 6px;
}}

/* ================= TABLES ================= */
table {{
    width: 100%;
    border-collapse: collapse;
    margin: 16px 0;
    font-size: 8.5pt;
    page-break-inside: avoid;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}}

th {{
    background-color: #1e293b;
    color: #ffffff;
    font-weight: 600;
    text-align: left;
    padding: 7px 10px;
    border: 1px solid #1e293b;
}}

td {{
    padding: 7px 10px;
    border: 1px solid #e2e8f0;
    vertical-align: top;
}}

tr:nth-child(even) {{
    background-color: #f8fafc;
}}

/* ================= CODE BLOCKS ================= */
code {{
    font-family: "Cascadia Code", "Consolas", "BIZ UDGothic", "MS Gothic", "Courier New", monospace;
    font-size: 8.5pt;
    background-color: #f1f5f9;
    color: #0f172a;
    padding: 2px 5px;
    border-radius: 4px;
    border: 1px solid #e2e8f0;
}}

pre {{
    background-color: #f8fafc;
    border: 1px solid #cbd5e1;
    border-radius: 6px;
    padding: 12px 16px;
    font-size: 8pt;
    line-height: 1.45;
    overflow: hidden;
    page-break-inside: avoid;
    white-space: pre-wrap;
    word-break: break-all;
    margin: 14px 0;
}}

pre code {{
    background: transparent;
    border: none;
    padding: 0;
    font-size: inherit;
    color: inherit;
}}

/* ================= CALLOUTS / ALERTS ================= */
.callout {{
    border-radius: 6px;
    padding: 12px 16px;
    margin: 16px 0;
    page-break-inside: avoid;
}}

.callout-title {{
    font-weight: 700;
    font-size: 9pt;
    letter-spacing: 0.5px;
    margin-bottom: 6px;
    display: flex;
    align-items: center;
    gap: 6px;
}}

.callout-body {{
    font-size: 9pt;
    line-height: 1.5;
}}

.callout-body > p:last-child {{
    margin-bottom: 0;
}}

.callout-note {{ background: #eff6ff; border-left: 4px solid #3b82f6; color: #1e3a8a; }}
.callout-note .callout-title {{ color: #1d4ed8; }}

.callout-tip {{ background: #ecfdf5; border-left: 4px solid #10b981; color: #064e3b; }}
.callout-tip .callout-title {{ color: #047857; }}

.callout-important {{ background: #f5f3ff; border-left: 4px solid #8b5cf6; color: #4c1d95; }}
.callout-important .callout-title {{ color: #6d28d9; }}

.callout-warning {{ background: #fffbeb; border-left: 4px solid #f59e0b; color: #78350f; }}
.callout-warning .callout-title {{ color: #b45309; }}

.callout-caution {{ background: #fef2f2; border-left: 4px solid #ef4444; color: #7f1d1d; }}
.callout-caution .callout-title {{ color: #b91c1c; }}

/* ================= IMAGES & DIAGRAMS ================= */
img {{
    max-width: 100%;
    height: auto;
    border-radius: 6px;
    border: 1px solid #cbd5e1;
    margin: 14px 0;
    display: block;
}}

.mermaid {{
    text-align: center;
    margin: 20px 0;
    page-break-inside: avoid;
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 16px;
}}

/* ================= MISC ================= */
hr {{
    border: none;
    border-top: 1px solid #e2e8f0;
    margin: 20px 0;
}}

blockquote {{
    border-left: 3px solid #cbd5e1;
    margin: 12px 0;
    padding: 6px 14px;
    color: #475569;
    background: #f8fafc;
}}

ul, ol {{
    padding-left: 22px;
    margin: 10px 0;
}}

li {{
    margin-bottom: 4px;
}}
</style>

<script>
{mermaid_js_content}
</script>
<script>
document.addEventListener('DOMContentLoaded', function() {{
    if (typeof mermaid !== 'undefined') {{
        mermaid.initialize({{
            startOnLoad: true,
            theme: 'default',
            securityLevel: 'loose',
            sequence: {{
                actorMargin: 50,
                showSequenceNumbers: false
            }},
            flowchart: {{
                useMaxWidth: true,
                htmlLabels: true,
                curve: 'basis'
            }}
        }});
    }}
}});
</script>
</head>
<body>

<!-- COVER PAGE -->
<div class="cover-page">
    <div class="cover-top">
        <div class="cover-badge-row">
            <span class="badge-pill badge-blue">Varian Medical Systems Eclipse</span>
            <span class="badge-pill badge-purple">ESAPI v15.6 / v16.1</span>
            <span class="badge-pill badge-green">Production Ready (v2.0.3)</span>
        </div>
        <div class="cover-title-group">
            <h1 class="cover-product">AutoStructureMaker</h1>
            <div class="cover-version">Version 2.0.3 (Released: 2026-09-07)</div>
            <div class="cover-subtitle">総合技術・臨床運用マニュアル</div>
            <div class="cover-desc">
                本マニュアルは、放射線治療計画装置 Varian Eclipse における高精度輪郭自動作成・論理演算・マージン生成支援プラグイン 
                <strong>AutoStructureMaker</strong> の導入、臨床コミッショニング手順、操作仕様、構造化 XML テンプレート記述法、
                システムアーキテクチャ、およびトラブルシューティングを網羅した公式技術文書です。
            </div>
        </div>

        <div class="cover-spec-grid">
            <div class="spec-card">
                <div class="spec-label">Target Environment</div>
                <div class="spec-val">Varian Eclipse v15.6 / v16.1 (ESAPI)</div>
            </div>
            <div class="spec-card">
                <div class="spec-label">Execution Framework</div>
                <div class="spec-val">Microsoft .NET Framework 4.6.1 (x64)</div>
            </div>
            <div class="spec-card">
                <div class="spec-label">Clinical Acceptance Standard</div>
                <div class="spec-val">AAPM TG-275 / MPPG 5.a 準拠</div>
            </div>
            <div class="spec-card">
                <div class="spec-label">Validation Status</div>
                <div class="spec-val">60/60 Tests Passed (100% PASS)</div>
            </div>
        </div>
    </div>

    <div class="cover-bottom">
        <div>Department of Radiation Oncology & Medical Physics</div>
        <div>Document ID: ASM-MAN-2026-V203 &bull; September 7, 2026</div>
    </div>
</div>

<!-- TABLE OF CONTENTS -->
<div class="toc-page">
    <div class="toc-header">
        <h2>目次 (Table of Contents)</h2>
    </div>
    <div class="toc-list">
        {toc_html}
    </div>
</div>

<!-- CHAPTERS CONTENT -->
{''.join(rendered_chapters_html)}

</body>
</html>
"""
    return html_template

def main():
    print("[1/3] Generating integrated HTML document...")
    html_content = build_html_document()
    
    temp_html = os.path.join(tempfile.gettempdir(), "AutoStructureMaker_Manual_Build.html")
    with open(temp_html, "w", encoding="utf-8") as f:
        f.write(html_content)
    print(f"      Temporary HTML saved: {temp_html} ({len(html_content):,} bytes)")

    print("[2/3] Printing HTML to PDF via headless Google Chrome...")
    temp_pdf = os.path.join(tempfile.gettempdir(), "AutoStructureMaker_Manual_Temp.pdf")
    if os.path.exists(temp_pdf):
        os.remove(temp_pdf)

    cmd = [
        CHROME_PATH,
        "--headless=new",
        "--disable-gpu",
        "--lang=ja",
        f"--print-to-pdf={temp_pdf}",
        "--no-pdf-header-footer",
        "--run-all-compositor-stages-before-draw",
        "--virtual-time-budget=5000",
        temp_html
    ]

    res = subprocess.run(cmd, capture_output=True)
    if res.returncode != 0 or not os.path.exists(temp_pdf):
        print(f"[ERROR] Chrome PDF generation failed! Return code: {res.returncode}")
        print("Stderr:", res.stderr.decode("utf-8", errors="ignore"))
        return 1

    print(f"      PDF rendered successfully! Size: {os.path.getsize(temp_pdf):,} bytes")

    print("[3/3] Deploying PDF to workspace and docs directory...")
    with open(temp_pdf, "rb") as src:
        pdf_bytes = src.read()

    with open(OUTPUT_PDF_ROOT, "wb") as dst:
        dst.write(pdf_bytes)
    print(f"      [OK] Root PDF: {OUTPUT_PDF_ROOT}")

    os.makedirs(os.path.dirname(OUTPUT_PDF_DOCS), exist_ok=True)
    with open(OUTPUT_PDF_DOCS, "wb") as dst:
        dst.write(pdf_bytes)
    print(f"      [OK] Docs PDF: {OUTPUT_PDF_DOCS}")

    os.makedirs(os.path.dirname(OUTPUT_PDF_RES), exist_ok=True)
    with open(OUTPUT_PDF_RES, "wb") as dst:
        dst.write(pdf_bytes)
    print(f"      [OK] Resources PDF: {OUTPUT_PDF_RES}")

    # Inspect with PyMuPDF
    try:
        import fitz
        doc = fitz.open(OUTPUT_PDF_ROOT)
        print("\n========================================================")
        print(f"  [SUCCESS] Manual PDF Generated Successfully!")
        print(f"  Total Pages: {len(doc)} pages")
        print(f"  File Size:   {len(pdf_bytes) / 1024 / 1024:.2f} MB ({len(pdf_bytes):,} bytes)")
        print("========================================================")
    except Exception as e:
        print("Could not inspect with fitz:", e)

    return 0

if __name__ == "__main__":
    exit(main())
