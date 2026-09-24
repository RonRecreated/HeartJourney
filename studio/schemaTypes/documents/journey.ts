import {defineField, defineType} from 'sanity'
import { HeartIcon } from '@sanity/icons/Heart'

export const journeyType = defineType({
  name: 'journey',
  title: 'Journey',
  type: 'document',
  icon: HeartIcon,

  fieldsets: [
    {
      name: 'basic',
      title: 'Basic Information'
    },
    {
      name: 'content',
      title: 'Journey Content'
    },
    {
      name: 'branding',
      title: 'Visual Identity'
    },
    {
      name: 'publishing',
      title: 'Publishing'
    }
  ],

  fields: [

    defineField({
      name: 'title',
      title: 'Title',
      type: 'string',
      fieldset: 'basic',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'slug',
      title: 'Slug',
      type: 'slug',
      fieldset: 'basic',
      options: {
        source: 'title',
        maxLength: 96
      },
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'summary',
      title: 'Summary',
      type: 'text',
      rows: 3,
      fieldset: 'content',
      validation: Rule => Rule.max(300)
    }),

    defineField({
      name: 'introduction',
      title: 'Introduction',
      type: 'array',
      of: [{ type: 'block' }],
      fieldset: 'content'
    }),

    defineField({
      name: 'purpose',
      title: 'Purpose',
      type: 'text',
      rows: 4,
      fieldset: 'content'
    }),

    defineField({
      name: 'heroImage',
      title: 'Hero Image',
      type: 'image',
      options: {
        hotspot: true
      },
      fieldset: 'branding'
    }),

    defineField({
      name: 'theme',
      title: 'Theme',
      type: 'string',
      options: {
        list: [
          { title: 'Olive', value: 'olive' },
          { title: 'Ocean', value: 'ocean' },
          { title: 'Sunrise', value: 'sunrise' },
          { title: 'Forest', value: 'forest' },
          { title: 'Royal', value: 'royal' }
        ]
      },
      fieldset: 'branding'
    }),

    defineField({
      name: 'icon',
      title: 'Icon',
      description: 'Internal icon identifier used by the web application.',
      type: 'string',
      fieldset: 'branding'
    }),

    defineField({
      name: 'sortOrder',
      title: 'Sort Order',
      type: 'number',
      initialValue: 1,
      fieldset: 'publishing'
    }),

    defineField({
      name: 'published',
      title: 'Published',
      type: 'boolean',
      initialValue: false,
      fieldset: 'publishing'
    })
  ],

  preview: {
    select: {
      title: 'title',
      media: 'heroImage'
    }
  }
})