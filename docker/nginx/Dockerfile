# The base image (and so the nginx version) is NGINX_IMAGE from docker/.env, passed as a build arg. It
# must be the unprivileged nginx build: the container runs as the nginx user and keeps its pid and caches
# in folders that user owns. It listens on 80 like any nginx; inside a container that needs no privilege
# once net.ipv4.ip_unprivileged_port_start is 0, which the compose file sets explicitly.
ARG NGINX_IMAGE
FROM ${NGINX_IMAGE}

USER root
RUN rm /etc/nginx/conf.d/default.conf
USER 101

COPY templates/ /etc/nginx/templates/
COPY snippets/security-headers.conf snippets/uploads-location.conf /etc/nginx/snippets/

EXPOSE 80

# /health is the API's liveness route, proxied by the first server block; the Host header is the
# client portal's own name, so the probe takes the same path a visitor's monitor does.
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -q -O /dev/null --header="Host: ${DOMAIN}" http://127.0.0.1:80/health || exit 1

CMD ["nginx", "-g", "daemon off;"]
