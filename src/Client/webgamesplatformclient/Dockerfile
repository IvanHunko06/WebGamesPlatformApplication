FROM node:23-alpine

WORKDIR /app

COPY package*.json .
COPY vite.config.js .
RUN npm install

COPY . .
EXPOSE 5173

ENTRYPOINT ["npm", "run", "dev", "--", "--host"]