// ==== ELEMENTOS DOM ====
const toggleTextBtn = document.getElementById("toggleTextBtn");
const toggleUrlBtn = document.getElementById("toggleUrlBtn");
const inputArea = document.getElementById("inputArea");
const slider = document.getElementById("slider");
const loaderOverlay = document.getElementById("loaderOverlay");
const conteudoPrincipal = document.getElementById("conteudoPrincipal");

// ==== FUNÇÕES UTILITÁRIAS ====
function autoResizeTextarea(textarea) {
    textarea.addEventListener('input', () => {
        textarea.style.height = 'auto';
        textarea.style.height = Math.min(textarea.scrollHeight, 208) + 'px';
    });
}

function atualizarBotaoEnvio() {
    const form = document.querySelector('form:not(.inline-block)');
    const botaoEnviar = form?.querySelector('button[type="submit"]');
    const campoTexto = form?.querySelector('[name="texto"]');
    const valor = campoTexto?.value.trim() ?? "";

    if (botaoEnviar) {
        botaoEnviar.disabled = valor === "";
        botaoEnviar.classList.toggle('opacity-50', botaoEnviar.disabled);
        botaoEnviar.classList.toggle('cursor-not-allowed', botaoEnviar.disabled);
    }
}

function mostrarLoader() {
    loaderOverlay?.classList.remove("hidden");
    conteudoPrincipal?.classList.add("hidden");
}

function criarTextarea() {
    inputArea.innerHTML = `
      <textarea id="autoResizeTextarea"
                name="texto"
                rows="1"
                class="w-full overflow-hidden resize-none max-h-52 outline-none transition-all focus:text-text-primary"
                placeholder="Pergunte qualquer coisa..."></textarea>
    `;
    const newTextarea = document.getElementById("autoResizeTextarea");
    if (newTextarea) autoResizeTextarea(newTextarea);
}

function criarInputUrl() {
    inputArea.innerHTML = `
      <input type="url"
             name="texto"
             class="w-full rounded-lg border border-gray-300 px-4 py-2 outline-none focus:ring-none focus:border-[#9ec9bc] transition-all focus:text-text-primary"
             placeholder="Cole aqui o link para análise" />
    `;
}

function isValidUrl(string) {
    try {
        new URL(string);
        return true;
    } catch (_) {
        return false;
    }
}

function atualizarResultado(data) {
    const container = document.getElementById("conteudoPrincipal");
    let html = "";

    html += `<div class="mt-20 mb-28">
                <div class="flex mb-4 pb-3 w-full border-b border-gray-200">`;

    if (isValidUrl(data.enviado)) {
        html += `<a href="${data.enviado}" target="_blank" rel="noopener noreferrer"
                class="inline-flex items-center gap-4 px-3 py-2 rounded-lg bg-[#e0f3eb] border border-[#9ec9bc] text-sm text-primary transition-all break-all">
                <span class="line-clamp-1 hover:underline">${data.enviado}</span>
                </a>`;
    } else {
        html += `<div class="text-text-primary font-semibold text-lg rounded-xl w-full break-words">${data.enviado}</div>`;
    }

    html += `</div><div class="flex flex-col gap-4">`;

    // Classificação
    let colorClass = "";
    let icon = "";
    let textColor = "";

    if (data.status === "Informação verdadeira") {
        colorClass = "bg-green-100 text-green-600";
        icon = "circle-check";
        textColor = "text-green-700";
    } else if (data.status === "Informação falsa") {
        colorClass = "bg-red-100 text-red-600";
        icon = "x-octagon";
        textColor = "text-red-700";
    } else if (data.status === "Informação duvidosa") {
        colorClass = "bg-yellow-100 text-yellow-600";
        icon = "alert-triangle";
        textColor = "text-yellow-800";
    } else if (data.status === "erro") {
        colorClass = "bg-red-100 text-red-600";
        icon = "alert-circle";
        textColor = "text-red-700";
    }

    html += `<div class="flex items-center gap-3 rounded-xl w-fit">
                <div class="flex items-center justify-center w-10 h-10 rounded-lg ${colorClass}">
                    <i data-lucide="${icon}" class="w-6 h-6"></i>
                </div>
                <p class="${textColor} font-semibold text-lg">${data.status}</p>
            </div>`;

    if (data.explicacaoHtml && data.explicacaoHtml.length > 0) {
        html += `<div class="prose max-w-none [&_a]:text-blue-600 [&_a:hover]:underline [&_a]:break-words text-text-primary">
                    ${data.explicacaoHtml}
                </div>`;
    }

    if (data.referencias && data.referencias.length > 0) {
        data.referencias.forEach(ref => {
            html += `<a href="${ref.url}" target="_blank" rel="noopener noreferrer"
                            class="group bg-white border border-gray-200 p-3 rounded-md hover:bg-gray-50 hover:border-gray-300 transition-all duration-200 block">
                            <div class="flex items-center gap-2 mb-2">
                                <img src="${ref.faviconUrl}" class="w-4 h-4 mt-px rounded-full border" />
                                <span class="text-xs text-gray-500 group-hover:text-gray-700">${ref.nomeExibicao}</span>
                            </div>
                            <h3 class="text-sm font-medium text-gray-800 group-hover:underline group-hover:text-gray-900 line-clamp-2">
                                ${ref.titulo}
                            </h3>
                            <p class="text-sm text-gray-600 mt-1 group-hover:text-gray-800 leading-snug line-clamp-2">
                                ${ref.descricao}
                            </p>
                        </a>`;
        });
    }

    html += `</div></div>`;
    container.innerHTML = html;

    // Atualiza icons lucide
    if (window.lucide) {
        window.lucide.createIcons();
    }
}

// ==== EVENTOS ====
document.addEventListener("keydown", function (event) {
    if (event.key === "Enter" && !event.shiftKey) {
        const target = event.target;

        if (target.tagName === "TEXTAREA" && target.name === "texto") {
            const valor = target.value.trim();
            if (valor === "") {
                event.preventDefault();
                return;
            }

            event.preventDefault();
            mostrarLoader();
            target.closest("form")?.submit();
        }

        if (target.tagName === "INPUT" && target.name === "texto") {
            const valor = target.value.trim();
            if (valor === "") {
                event.preventDefault();
            }
        }
    }
});

if (toggleTextBtn) {
    toggleTextBtn.addEventListener("click", () => {
        toggleTextBtn.classList.remove("opacity-60");
        toggleUrlBtn.classList.add("opacity-60");
        slider.style.left = "2.5px";
        slider.style.width = "95px";
        criarTextarea();
        atualizarBotaoEnvio();
    });
}

if (toggleUrlBtn) {
    toggleUrlBtn.addEventListener("click", () => {
        toggleUrlBtn.classList.remove("opacity-60");
        toggleTextBtn.classList.add("opacity-60");
        slider.style.left = "calc(100% - 80px)";
        slider.style.width = "78px";
        criarInputUrl();
        atualizarBotaoEnvio();
    });
}

document.addEventListener("input", atualizarBotaoEnvio);
document.addEventListener("change", atualizarBotaoEnvio);

// ==== INICIALIZAÇÃO ====
document.addEventListener("DOMContentLoaded", () => {
    const initialTextarea = document.getElementById("autoResizeTextarea");
    if (initialTextarea) autoResizeTextarea(initialTextarea);
    atualizarBotaoEnvio();
});

// ==== SISTEMA DE LOADING DINÂMICO ====
const mensagensLoading = [
    "Buscando informações em fontes confiáveis...",
    "Consultando bases de dados oficiais...",
    "Verificando a veracidade da informação...",
    "Analisando credibilidade das fontes...",
    "Cruzando dados com agências de notícias...",
    "Comparando com fatos verificados...",
    "Checando em órgãos governamentais...",
    "Preparando análise detalhada...",
    "Finalizando verificação..."
];

function iniciarLoadingDinamico() {
    const elementoTexto = document.querySelector('.text-loader');
    if (!elementoTexto) return;

    let indiceAtual = 0;

    const trocarMensagem = () => {
        if (elementoTexto && indiceAtual < mensagensLoading.length) {
            // Efeito de fade out
            elementoTexto.style.opacity = '0.5';

            setTimeout(() => {
                elementoTexto.textContent = mensagensLoading[indiceAtual];
                elementoTexto.style.opacity = '1';
                indiceAtual++;
            }, 300);
        }
    };

    // Primeira troca após 2 segundos
    setTimeout(trocarMensagem, 2000);

    // Continua trocando a cada 3 segundos
    const intervaloMensagens = setInterval(() => {
        if (indiceAtual >= mensagensLoading.length) {
            // Reinicia o ciclo se necessário
            indiceAtual = 0;
        }
        trocarMensagem();
    }, 3000);

    return intervaloMensagens;
}

// ==== POLLING PARA RESULTADOS PENDENTES ====
// Este código só executa na página de resultado
if (typeof window !== 'undefined' && window.location.pathname.includes('/Resultado/')) {
    const urlParams = new URLSearchParams(window.location.search);
    const id = window.location.pathname.split('/').pop();

    // Verifica se existe um elemento que indica que está pendente
    const elementoPendente = document.querySelector('.animate-spin');

    if (elementoPendente && id) {
        // Inicia o sistema de loading dinâmico
        const intervaloMensagens = iniciarLoadingDinamico();

        const intervalo = setInterval(async () => {
            try {
                const res = await fetch(`/verificar/verificarstatus?id=${id}`);
                const data = await res.json();

                if (data.status !== "pendente") {
                    clearInterval(intervalo);
                    clearInterval(intervaloMensagens); // Para as mensagens de loading
                    atualizarResultado(data);
                }
            } catch (error) {
                console.error("Erro ao buscar status:", error);
                clearInterval(intervalo);
                clearInterval(intervaloMensagens);

                // Exibe mensagem de erro
                const container = document.getElementById("conteudoPrincipal");
                container.innerHTML = `
                    <div class="mt-20 mb-28 text-center">
                        <div class="flex items-center justify-center w-16 h-16 mx-auto mb-4 bg-red-100 rounded-full">
                            <i data-lucide="alert-circle" class="w-8 h-8 text-red-600"></i>
                        </div>
                        <h2 class="text-xl font-medium text-gray-700 mb-2">Erro ao carregar resultado</h2>
                        <p class="text-gray-500 text-sm mb-4">Ocorreu um erro ao buscar o resultado da verificação.</p>
                        <a href="/verificar" class="inline-block px-4 py-2 bg-primary text-white rounded-lg hover:opacity-85">
                            Tentar novamente
                        </a>
                    </div>
                `;

                if (window.lucide) {
                    window.lucide.createIcons();
                }
            }
        }, 2000);
    }
}