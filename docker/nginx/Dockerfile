# The base image (and so the nginx version) is NGINX_IMAGE from docker/.env, passed as a build arg.
ARG NGINX_IMAGE
FROM ${NGINX_IMAGE}

RUN rm /etc/nginx/conf.d/default.conf

COPY templates/ /etc/nginx/templates/

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
