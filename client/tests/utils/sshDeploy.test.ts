/**
 * Unit tests for the CI deployment snippets: the account's details are filled in, the private key
 * comes from the secret, and nothing is deleted on the server unless the customer opts in.
 */
import { describe, expect, it } from 'vitest'
import { DEPLOY_SECRET_NAME, githubDeployWorkflow, gitlabDeployJob } from '~/utils/sshDeploy'

/** An account to deploy to. */
const TARGET = { hostname: 'srv.example.test', port: 2299, username: 'web' }

describe('sshDeploy', () => {
  it('builds a GitHub workflow that rsyncs to the account on push to main', () => {
    const yml = githubDeployWorkflow(TARGET)

    expect(yml).toContain('branches: [main]')
    expect(yml).toContain(`\${{ secrets.${DEPLOY_SECRET_NAME} }}`)
    expect(yml).toContain(`"$${DEPLOY_SECRET_NAME}" > ~/.ssh/deploy_key`)
    expect(yml).toContain('ssh -p 2299 -i ~/.ssh/deploy_key -o StrictHostKeyChecking=accept-new')
    expect(yml).toContain('web@srv.example.test:public_html/')
  })

  it('builds a GitLab job with the same rsync, run only on main', () => {
    const yml = gitlabDeployJob(TARGET)

    expect(yml).toContain('$CI_COMMIT_BRANCH == "main"')
    expect(yml).toContain('apk add --no-cache openssh-client rsync')
    expect(yml).toContain('web@srv.example.test:public_html/')
  })

  it('never deletes server files unless the customer adds --delete', () => {
    for (const yml of [githubDeployWorkflow(TARGET), gitlabDeployJob(TARGET)]) {
      const rsync = yml.split('\n').find((line) => line.trim().startsWith('rsync'))
      expect(rsync).not.toContain('--delete')
      expect(yml).toContain('# Add --delete')
    }
  })
})
