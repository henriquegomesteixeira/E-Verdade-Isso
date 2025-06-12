// ==== ELEMENTOS DOM ====
const toggleTextBtn = document.getElementById("toggleTextBtn");
const toggleUrlBtn = document.getElementById("toggleUrlBtn");
const inputArea = document.getElementById("inputArea");
const slider = document.getElementById("slider");
const loaderOverlay = document.getElementById("loaderOverlay");
const conteudoPrincipal = document.getElementById("conteudoPrincipal");

// ==== CONFIG ====
const MAX_TEXTAREA_HEIGHT = 208;
const LOADING_MESSAGES = [
    "Analisando o conteúdo...",
    "Buscando fontes confiáveis...",
    "Comparando com notícias recentes...",
    "Procurando informações verdadeiras em sites conhecidos...",
    "Verificando se o que foi dito já aconteceu mesmo...",
    "Procurando explicações simples para você entender melhor...",
    "Consultando portais de notícias...",
    "Checando o que especialistas já disseram sobre isso...",
    "Organizando as informações para mostrar o resultado..."
];

// ==== UTILITÁRIOS ====
function autoResizeTextarea(textarea) {
    textarea.addEventListener("input", () => {
        textarea.style.height = "auto";
        textarea.style.height = Math.min(textarea.scrollHeight, MAX_TEXTAREA_HEIGHT) + "px";
    });
}

function atualizarBotaoEnvio() {
    const form = document.querySelector("form:not(.inline-block)");
    const botaoEnviar = form?.querySelector("button[type='submit']");
    const campoTexto = form?.querySelector("[name='texto']");
    const vazio = !campoTexto?.value.trim();

    if (botaoEnviar) {
        botaoEnviar.disabled = vazio;
        botaoEnviar.classList.toggle("opacity-50", vazio);
        botaoEnviar.classList.toggle("cursor-not-allowed", vazio);
    }
}

function mostrarLoader() {
    loaderOverlay?.classList.remove("hidden");
    conteudoPrincipal?.classList.add("hidden");
}

function criarTextarea() {
    inputArea.innerHTML = `
    <textarea id="autoResizeTextarea" name="texto" rows="1"
      class="w-full overflow-auto resize-none max-h-52 outline-none transition-all focus:text-text-primary"
      placeholder="Pergunte qualquer coisa..."></textarea>`;

    autoResizeTextarea(document.getElementById("autoResizeTextarea"));
}

function criarInputUrl() {
    inputArea.innerHTML = `
    <input type="url" name="texto"
      class="w-full rounded-lg border border-gray-300 px-4 py-2 outline-none focus:ring-none focus:border-[#9ec9bc] transition-all focus:text-text-primary"
      placeholder="Cole aqui o link para análise" />`;
}

function isValidUrl(str) {
    try {
        new URL(str);
        return true;
    } catch (_) {
        return false;
    }
}

function atualizarResultado(data) {
    const container = conteudoPrincipal;
    if (!container) return;

    const statusMap = {
        "Informação verdadeira": ["bg-green-100 text-green-600", "circle-check", "text-green-700"],
        "Informação falsa": ["bg-red-100 text-red-600", "x-octagon", "text-red-700"],
        "Informação duvidosa": ["bg-yellow-100 text-yellow-600", "alert-triangle", "text-yellow-800"],
        "Informação contextual": ["bg-blue-100 text-[#1b399d]", "info", "text-[#1b399d]"],
        erro: ["bg-red-100 text-red-600", "alert-circle", "text-red-700"]
    };

    const [bgClass, icon, textColor] = statusMap[data.status] || statusMap.erro;

    const headerHtml = isValidUrl(data.enviado)
        ? `<a href="${data.enviado}" target="_blank" rel="noopener noreferrer"
         class="inline-flex items-center gap-4 px-3 py-2 rounded-lg bg-[#e0f3eb] border border-[#9ec9bc] text-sm text-primary transition-all break-all">
         <span class="line-clamp-1 hover:underline">${data.enviado}</span></a>`
        : `<div class="text-text-primary font-semibold text-lg rounded-xl w-full break-words">${data.enviado}</div>`;

    const explicacaoHtml = data.explicacao
        ? `<div class="prose max-w-none [&_a]:text-blue-600 [&_a:hover]:underline [&_a]:break-words text-text-primary">
         ${data.explicacao}</div>`
        : "";

    const referenciasHtml = (data.referencias || []).map(ref => `
    <a href="${ref.url}" target="_blank" rel="noopener noreferrer"
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
    </a>`).join("");

    container.innerHTML = `
    <div class="mt-20 mb-32">
      <div class="flex mb-4 pb-3 w-full border-b border-gray-200">${headerHtml}</div>
      <div class="flex flex-col gap-4">
        <div class="flex items-center gap-3 rounded-xl w-fit">
          <div class="flex items-center justify-center w-10 h-10 rounded-lg ${bgClass}">
            <i data-lucide="${icon}" class="w-6 h-6"></i>
          </div>
          <p class="${textColor} font-semibold text-lg">${data.status}</p>
        </div>
        ${explicacaoHtml}
        ${referenciasHtml}
      </div>
    </div>`;

    window.lucide?.createIcons();
}

function iniciarLoadingDinamico() {
    const elementoTexto = document.querySelector(".text-loader");
    if (!elementoTexto) return;

    let indice = 0;
    const updateMessage = () => {
        elementoTexto.style.opacity = "0.5";
        setTimeout(() => {
            elementoTexto.textContent = LOADING_MESSAGES[indice];
            elementoTexto.style.opacity = "1";
            indice = (indice + 1) % LOADING_MESSAGES.length;
        }, 300);
    };

    setTimeout(updateMessage, 2000);
    return setInterval(updateMessage, 3000);
}

function configurarEventos() {
    document.addEventListener("keydown", (event) => {
        if (event.key === "Enter" && !event.shiftKey) {
            const el = event.target;
            if (["TEXTAREA", "INPUT"].includes(el.tagName) && el.name === "texto" && el.value.trim()) {
                event.preventDefault();
                mostrarLoader();
                el.closest("form")?.submit();
            }
        }
    });

    toggleTextBtn?.addEventListener("click", () => {
        toggleTextBtn.classList.remove("opacity-60");
        toggleUrlBtn.classList.add("opacity-60");
        slider.style.left = "2.5px";
        slider.style.width = "95px";
        criarTextarea();
        atualizarBotaoEnvio();
    });

    toggleUrlBtn?.addEventListener("click", () => {
        toggleUrlBtn.classList.remove("opacity-60");
        toggleTextBtn.classList.add("opacity-60");
        slider.style.left = "calc(100% - 80px)";
        slider.style.width = "78px";
        criarInputUrl();
        atualizarBotaoEnvio();
    });

    document.addEventListener("input", atualizarBotaoEnvio);
    document.addEventListener("change", atualizarBotaoEnvio);
}

function iniciarVerificacaoPendente() {
    const isResultado = window.location.pathname.includes("/Resultado/");
    if (!isResultado) return;

    const id = window.location.pathname.split("/").pop();
    const spinner = document.querySelector(".animate-spin");

    if (spinner && id) {
        const mensagens = iniciarLoadingDinamico();

        const intervalo = setInterval(async () => {
            try {
                const res = await fetch(`/verificar/verificarstatus?id=${id}`);
                const data = await res.json();

                if (data.status !== "pendente") {
                    clearInterval(intervalo);
                    clearInterval(mensagens);
                    atualizarResultado(data);
                }
            } catch (error) {
                console.error("Erro ao buscar status:", error);
                clearInterval(intervalo);
                clearInterval(mensagens);
                conteudoPrincipal.innerHTML = `
          <div class="mt-20 mb-32 text-center">
            <div class="flex items-center justify-center w-16 h-16 mx-auto mb-4 bg-red-100 rounded-full">
              <i data-lucide="alert-circle" class="w-8 h-8 text-red-600"></i>
            </div>
            <h2 class="text-xl font-medium text-gray-700 mb-2">Erro ao carregar resultado</h2>
            <p class="text-gray-500 text-sm mb-4">Ocorreu um erro ao buscar o resultado da verificação.</p>
            <a href="/verificar" class="inline-block px-4 py-2 bg-primary text-white rounded-lg hover:opacity-85">
              Tentar novamente
            </a>
          </div>`;
                window.lucide?.createIcons();
            }
        }, 2000);
    }
}

// ==== INICIALIZAÇÃO ====
document.addEventListener("DOMContentLoaded", () => {
    autoResizeTextarea(document.getElementById("autoResizeTextarea"));
    atualizarBotaoEnvio();
    configurarEventos();
    iniciarVerificacaoPendente();
});
