# É Verdade Isso? 🔍

Uma plataforma inteligente de verificação de fatos que combate a desinformação usando IA avançada e fontes confiáveis em tempo real.

![EVerdadeIsso](https://github.com/user-attachments/assets/acf5d9db-4968-4bc2-936b-ba7327f2891b)

## 🎯 Sobre o Projeto

**É Verdade Isso?** é uma aplicação web desenvolvida para combater fake news e desinformação, oferecendo verificação instantânea de notícias e informações através de inteligência artificial conectada a fontes jornalísticas confiáveis.

### 🌟 Características Principais

- **Verificação em Tempo Real**: Análise instantânea de textos e URLs
- **Fontes Confiáveis**: Integração com portais jornalísticos estabelecidos
- **Interface Intuitiva**: Design responsivo e experiência de usuário otimizada
- **Classificação Transparente**: Sistema claro de categorização (Verdadeiro, Falso, Duvidoso, Contextual)
- **Referências Completas**: Links diretos para todas as fontes utilizadas na verificação

## 🚀 Funcionalidades

### ✅ Verificação de Conteúdo
- Análise de textos livres
- Verificação de URLs de notícias
- Processamento assíncrono para melhor performance
- Sistema de cache para otimização

### 🎨 Interface do Usuário
- Design moderno com Tailwind CSS
- Animações e transições suaves
- Totalmente responsivo para todos os dispositivos

### 📊 Sistema de Classificação
- **Informação Verdadeira**: Confirmada por múltiplas fontes
- **Informação Falsa**: Contradita por evidências verificáveis
- **Informação Duvidosa**: Dados insuficientes ou controversos
- **Informação Contextual**: Respostas explicativas baseadas em fatos

### 🔗 Integração com APIs
- **Perplexity AI**: Para análise avançada de conteúdo
- **OpenAI**: Para geração de sugestões e dicas educativas
- Sistema robusto de tratamento de erros

## 🛠️ Stack Tecnológico

### **Backend (.NET)**
- **ASP.NET Core 8.0** - Framework web principal
- **C# 12** - Linguagem de programação
- **MVC Pattern** - Arquitetura Model-View-Controller
- **Razor Pages** - Engine de renderização server-side
- **Memory Cache** - Cache em memória para otimização
- **Dependency Injection** - Injeção de dependência nativa
- **HTTP Client Factory** - Cliente HTTP configurável

### **Frontend**
- **HTML5 Semântico** - Estrutura acessível
- **CSS3 + Tailwind CSS 4.0** - Estilização moderna e responsiva
- **JavaScript ES6+** - Interatividade client-side
- **Lucide Icons** - Biblioteca de ícones SVG
- **Google Fonts** - Tipografia (Inter + Poppins)

### **Integrações de IA**
- **Perplexity AI API** - Verificação de fatos com IA
- **OpenAI GPT-4** - Geração de sugestões educativas
- **JSON Schema Validation** - Estruturação de respostas
- **RESTful APIs** - Comunicação com serviços externos

## 🏗️ Arquitetura

### Padrões Implementados
- **MVC (Model-View-Controller)** - Separação de responsabilidades
- **Service Layer Pattern** - Lógica de negócio isolada
- **Dependency Injection** - Inversão de controle
- **DTO Pattern** - Transferência segura de dados
- **Mapper Pattern** - Transformação de dados
- **Factory Pattern** - Criação de clientes HTTP

### Estrutura do Projeto
```
everdadeisso/
├── Controllers/          # Controladores MVC
│   ├── HomeController.cs
│   └── VerificarController.cs
├── Services/            # Lógica de negócio
│   ├── OpenAIService.cs
│   └── VerificacaoService.cs
├── Integrations/        # Clientes de APIs externas
│   ├── OpenAIClient.cs
│   └── PerplexityClient.cs
├── Interfaces/          # Contratos e abstrações
├── Models/              # Modelos de dados e ViewModels
│   ├── DTOs/
│   └── ViewModels/
├── Mappers/             # Transformação de dados
├── Views/               # Templates Razor
│   ├── Home/
│   ├── Verificar/
│   └── Shared/
├── wwwroot/             # Arquivos estáticos
│   ├── css/
│   ├── js/
│   └── img/
└── Dockerfile           # Configuração Docker
```

### Fluxo de Dados
1. **Controller** recebe requisição HTTP
2. **Service** processa lógica de negócio
3. **Integration** comunica com APIs externas
4. **Mapper** transforma dados para ViewModels
5. **View** renderiza resposta para o usuário

## 🔒 Segurança e Privacidade

### Proteção de Dados
- **Não armazenamento**: Dados não são persistidos permanentemente
- **Cache Temporário**: Informações mantidas apenas por 5 minutos
- **Sem Cookies**: Nenhum rastreamento de usuários
- **LGPD Compliant**: Política de privacidade transparente

### Validações
- **Limite de Caracteres**: Máximo de 2.000 caracteres por consulta
- **Sanitização**: Limpeza de dados de entrada
- **Rate Limiting**: Controle de uso através de cache

## 📱 Responsividade

- **Mobile First**: Design otimizado para dispositivos móveis
- **Breakpoints**: Adaptação para tablets e desktops
- **Touch Friendly**: Elementos adequados para toque
- **Performance Mobile**: Otimização para conexões lentas

## 🚀 Performance

### Otimizações Implementadas
- **Async/Await**: Processamento assíncrono
- **Memory Cache**: Cache em memória para respostas
- **Lazy Loading**: Carregamento sob demanda
- **Minificação**: CSS e JS otimizados
- **Compression**: Compressão de assets

## 📈 Métricas e Monitoramento

### Funcionalidades de Observabilidade
- **Request Tracking**: Rastreamento de requisições
- **Error Logging**: Log estruturado de erros
- **Performance Metrics**: Métricas de performance
- **Health Endpoints**: Endpoints de saúde da aplicação

## 🎯 Objetivos do Projeto

### Impacto Social
- **Combate à Desinformação**: Ferramenta acessível para verificação de fatos
- **Educação Digital**: Conscientização sobre fake news
- **Transparência**: Fontes sempre visíveis e verificáveis
- **Democratização**: Acesso gratuito à verificação de informações

### Objetivos Técnicos
- **Escalabilidade**: Arquitetura preparada para crescimento
- **Manutenibilidade**: Código organizado e documentado
- **Performance**: Resposta rápida e eficiente
- **Confiabilidade**: Sistema robusto e estável

## 👨‍💻 Desenvolvedor

**Henrique Gomes Teixeira**
- 🔗 [LinkedIn](https://www.linkedin.com/in/henriquegomesteixeira/)
- 🐙 [GitHub](https://github.com/henriquegomesteixeira)
