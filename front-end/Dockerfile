# =========================
# BUILD STAGE
# =========================
FROM node:20-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# =========================
# RUNTIME STAGE
# =========================
# estágio final apenas para exportar dist
FROM alpine:3.19
WORKDIR /dist
COPY --from=build /app/dist .
CMD ["sh"]