/**
 * Schema.org structured data composable
 * Provides JSON-LD markup for SEO
 *
 * @module composables/useSchemaOrg
 */

import { APP_LOCALES } from '~/constants/appLocales'

/** A service as the service schema describes it. */
interface SchemaService {
  /** Service name. */
  name: string
  /** Short description. */
  description: string
  /** Address of the service page, absolute or site-relative. */
  url: string
  /** Kind of service, e.g. `Web Hosting`. */
  serviceType: string
}

/** A software product as the product schema describes it. */
interface SchemaProduct {
  /** Product name. */
  name: string
  /** Short description. */
  description: string
  /** Address of the product page, absolute or site-relative. */
  url: string
  /** Product image. */
  image?: string
}

/** One step of a breadcrumb trail. */
interface SchemaBreadcrumb {
  /** Label of the step. */
  name: string
  /** Address of the step, absolute or site-relative. */
  url: string
}

/** One question and its answer. */
interface SchemaFaq {
  /** The question. */
  question: string
  /** The answer. */
  answer: string
}

/**
 * Builds the JSON-LD documents of the public site and injects them into the head.
 *
 * @returns The base URL and one builder per schema type, plus the injector.
 */
export const useSchemaOrg = () => {
  const config = useRuntimeConfig()
  const baseUrl = config.public.baseUrl || 'https://yourdomain.com'
  const contactSettings = usePortalSettings()
  // The operator's name and tagline. The address, phone, founding date, opening hours
  // and rating below are still Innovayse's own and belong in settings too.
  // TODO(phase 3): move the organisation details into portal.* settings.
  const { name: siteName, tagline: siteTagline } = useSiteIdentity()

  /**
   * Organization schema
   */
  const organizationSchema = () => {
    return {
      '@context': 'https://schema.org',
      '@type': 'Organization',
      '@id': `${baseUrl}/#organization`,
      name: siteName.value,
      url: baseUrl,
      logo: {
        '@type': 'ImageObject',
        url: `${baseUrl}/logo.png`
      },
      description: siteTagline.value || 'Web hosting, domains and servers with online billing and a client portal',
      foundingDate: '2016',
      contactPoint: {
        '@type': 'ContactPoint',
        telephone: '+374-33-731673',
        contactType: 'customer service',
        availableLanguage: ['English', 'Russian', 'Armenian']
      },
      address: {
        '@type': 'PostalAddress',
        addressCountry: 'AM',
        addressLocality: 'Yerevan'
      },
      // Built from the operator's contact settings rather than fixed, so a
      // self-hosted install does not publish Innovayse's own channels as its
      // own structured data. Unset channels are omitted rather than empty.
      sameAs: [
        contactSettings.get('portal.contact.telegram', 'portalTelegram')
          ? `https://t.me/${contactSettings.get('portal.contact.telegram', 'portalTelegram')}`
          : '',
        contactSettings.get('portal.contact.whatsapp', 'portalWhatsapp')
          ? `https://wa.me/${contactSettings.get('portal.contact.whatsapp', 'portalWhatsapp')}`
          : '',
      ].filter(Boolean),
      email: 'contact@yourdomain.com'
    }
  }

  /**
   * LocalBusiness schema
   */
  const localBusinessSchema = () => {
    return {
      '@context': 'https://schema.org',
      '@type': 'LocalBusiness',
      '@id': `${baseUrl}/#organization`,
      name: siteName.value,
      image: `${baseUrl}/logo.png`,
      url: baseUrl,
      telephone: '+374-33-731673',
      email: 'contact@yourdomain.com',
      address: {
        '@type': 'PostalAddress',
        addressCountry: 'AM',
        addressLocality: 'Yerevan'
      },
      geo: {
        '@type': 'GeoCoordinates',
        latitude: 40.175837,
        longitude: 44.512906
      },
      openingHoursSpecification: {
        '@type': 'OpeningHoursSpecification',
        dayOfWeek: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'],
        opens: '09:00',
        closes: '18:00'
      },
      priceRange: '$$',
      aggregateRating: {
        '@type': 'AggregateRating',
        ratingValue: '4.9',
        reviewCount: '150'
      }
    }
  }

  /**
   * WebSite schema with search action
   */
  const websiteSchema = () => {
    return {
      '@context': 'https://schema.org',
      '@type': 'WebSite',
      '@id': `${baseUrl}/#website`,
      url: baseUrl,
      name: siteName.value,
      description: siteTagline.value || 'Web hosting, domains and servers with online billing and a client portal',
      publisher: {
        '@id': `${baseUrl}/#organization`
      },
      inLanguage: [...APP_LOCALES]
    }
  }

  /**
   * Service schema
   */
  const serviceSchema = (service: SchemaService) => {
    return {
      '@context': 'https://schema.org',
      '@type': 'Service',
      name: service.name,
      description: service.description,
      url: service.url.startsWith('http') ? service.url : `${baseUrl}${service.url}`,
      serviceType: service.serviceType,
      provider: {
        '@id': `${baseUrl}/#organization`
      },
      areaServed: {
        '@type': 'Country',
        name: 'Worldwide'
      }
    }
  }

  /**
   * Product schema (for SaaS products)
   */
  const productSchema = (product: SchemaProduct) => {
    return {
      '@context': 'https://schema.org',
      '@type': 'SoftwareApplication',
      name: product.name,
      description: product.description,
      url: product.url.startsWith('http') ? product.url : `${baseUrl}${product.url}`,
      applicationCategory: 'BusinessApplication',
      operatingSystem: 'Web',
      offers: {
        '@type': 'AggregateOffer',
        priceCurrency: 'USD',
        lowPrice: '99',
        highPrice: '999',
        offerCount: '3'
      },
      aggregateRating: {
        '@type': 'AggregateRating',
        ratingValue: '4.8',
        reviewCount: '120'
      },
      provider: {
        '@id': `${baseUrl}/#organization`
      }
    }
  }

  /**
   * BreadcrumbList schema
   */
  const breadcrumbSchema = (items: SchemaBreadcrumb[]) => {
    return {
      '@context': 'https://schema.org',
      '@type': 'BreadcrumbList',
      itemListElement: items.map((item, index) => ({
        '@type': 'ListItem',
        position: index + 1,
        name: item.name,
        item: item.url.startsWith('http') ? item.url : `${baseUrl}${item.url}`
      }))
    }
  }

  /**
   * FAQ schema
   */
  const faqSchema = (faqs: SchemaFaq[]) => {
    return {
      '@context': 'https://schema.org',
      '@type': 'FAQPage',
      mainEntity: faqs.map(faq => ({
        '@type': 'Question',
        name: faq.question,
        acceptedAnswer: {
          '@type': 'Answer',
          text: faq.answer
        }
      }))
    }
  }

  /**
   * Inject schema into head
   */
  const injectSchema = (schema: object | object[]) => {
    const schemas = Array.isArray(schema) ? schema : [schema]

    useHead({
      script: schemas.map(s => ({
        type: 'application/ld+json',
        // innerHTML, not `children`: the current unhead renders an unknown key as an
        // attribute, and the structured data was landing in children="…" on an empty
        // <script> — emitted nowhere a crawler reads.
        innerHTML: JSON.stringify(s)
      }))
    })
  }

  return {
    baseUrl,
    organizationSchema,
    localBusinessSchema,
    websiteSchema,
    serviceSchema,
    productSchema,
    breadcrumbSchema,
    faqSchema,
    injectSchema
  }
}
