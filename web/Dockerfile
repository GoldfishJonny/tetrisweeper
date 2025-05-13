FROM nginx:alpine
RUN rm /etc/nginx/conf.d/default.conf

COPY web/nginx.conf /etc/nginx/conf.d/web.conf

COPY ../Build /usr/share/nginx/html