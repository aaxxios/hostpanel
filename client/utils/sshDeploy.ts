/**
 * Ready-to-paste CI files that deploy a site to its hosting account over SSH with rsync. The
 * panel fills in the account's own hostname, port and username, so a customer copies the file
 * as it is and adds one secret.
 *
 * @module utils/sshDeploy
 */

/** Where an account's site is served from, relative to its home directory. */
const DOCUMENT_ROOT = 'public_html/'

/** The command that creates a key pair just for CI, without a passphrase. */
export const DEPLOY_KEYGEN_COMMAND = 'ssh-keygen -t ed25519 -C "github-deploy" -f deploy_key -N ""'

/** The repository secret that holds the private deploy key. */
export const DEPLOY_SECRET_NAME = 'SSH_PRIVATE_KEY'

/** Where an account is reached for a deployment. */
export interface DeployTarget {
  /** The SSH hostname. */
  hostname: string
  /** The SSH port. */
  port: number
  /** The hosting account's username. */
  username: string
}

/**
 * The rsync line both CI files run. `--delete` is left out on purpose and explained in a comment:
 * it removes files on the server that are not in the repository, which is what a mirror wants
 * and what a site with uploaded files must not do.
 *
 * @param target - Where the site is deployed.
 * @param indent - Leading spaces for each line, to sit inside the YAML block.
 * @returns The commented rsync command.
 */
const rsyncLines = (target: DeployTarget, indent: string): string => [
  `${indent}# Add --delete after -az to also remove server files that are not in the repository.`,
  `${indent}# Leave it out if the site keeps uploads or other files that only exist on the server.`,
  `${indent}rsync -az --exclude .git -e "ssh -p ${target.port} -i ~/.ssh/deploy_key -o StrictHostKeyChecking=accept-new" ./ ${target.username}@${target.hostname}:${DOCUMENT_ROOT}`
].join('\n')

/**
 * A GitHub Actions workflow that deploys the repository on every push to main.
 *
 * @param target - Where the site is deployed.
 * @returns The contents of `.github/workflows/deploy.yml`.
 */
export const githubDeployWorkflow = (target: DeployTarget): string => `name: Deploy
on:
  push:
    branches: [main]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Upload the site
        env:
          ${DEPLOY_SECRET_NAME}: \${{ secrets.${DEPLOY_SECRET_NAME} }}
        run: |
          mkdir -p ~/.ssh
          printf '%s\\n' "$${DEPLOY_SECRET_NAME}" > ~/.ssh/deploy_key
          chmod 600 ~/.ssh/deploy_key
${rsyncLines(target, '          ')}
`

/**
 * A GitLab CI job that deploys the repository on every push to main.
 *
 * @param target - Where the site is deployed.
 * @returns The job to put in `.gitlab-ci.yml`.
 */
export const gitlabDeployJob = (target: DeployTarget): string => `deploy:
  image: alpine:3.20
  rules:
    - if: $CI_COMMIT_BRANCH == "main"
  before_script:
    - apk add --no-cache openssh-client rsync
    - mkdir -p ~/.ssh
    - printf '%s\\n' "$${DEPLOY_SECRET_NAME}" > ~/.ssh/deploy_key
    - chmod 600 ~/.ssh/deploy_key
  script:
    - |
${rsyncLines(target, '      ')}
`
