FROM node:22-alpine AS build
WORKDIR /app

ARG API_BASE_URL=http://localhost:5000
ARG ONLYOFFICE_URL=http://localhost:8080

COPY ./frontend/package.json ./
COPY ./frontend/angular.json ./
COPY ./frontend/tsconfig.json ./
COPY ./frontend/tsconfig.app.json ./
RUN npm install

COPY ./frontend/src ./src
COPY ./frontend/public ./public
RUN sed -i "s|http://localhost:5000|${API_BASE_URL}|g" ./src/environments/environment.prod.ts && \
    sed -i "s|/api-proxy|${API_BASE_URL}|g" ./src/environments/environment.prod.ts && \
    sed -i "s|/ds|${ONLYOFFICE_URL}|g" ./src/environments/environment.prod.ts && \
    npm run build

FROM node:22-alpine AS runtime
WORKDIR /app
RUN npm install -g http-server
COPY --from=build /app/dist/onlyoffice-collab-web/browser ./dist
EXPOSE 4200
CMD ["http-server", "dist", "-p", "4200", "-a", "0.0.0.0", "-c-1"]
