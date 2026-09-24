import { defineField, defineType } from 'sanity'
import { CommentIcon } from '@sanity/icons/Comment'

export const reflectionPromptType = defineType({
  name: 'reflectionPrompt',
  title: 'Reflection Prompt',
  type: 'document',
  icon: CommentIcon,

  fieldsets: [
    {
      name: 'identity',
      title: 'Identity'
    },
    {
      name: 'context',
      title: 'Context'
    },
    {
      name: 'prompt',
      title: 'Prompt'
    },
    {
      name: 'answers',
      title: 'Answer Choices'
    },
    {
      name: 'notes',
      title: 'Optional Notes'
    },
    {
      name: 'guidance',
      title: 'Guidance'
    },
    {
      name: 'publishing',
      title: 'Publishing'
    }
  ],

  fields: [
    // -------------------------
    // Identity
    // -------------------------

    defineField({
      name: 'title',
      title: 'Title',
      type: 'string',
      fieldset: 'identity',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'slug',
      title: 'Slug',
      type: 'slug',
      fieldset: 'identity',
      options: {
        source: 'title',
        maxLength: 96
      },
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'type',
      title: 'Prompt Type',
      type: 'string',
      fieldset: 'identity',
      options: {
        list: [
          { title: 'Reflective', value: 'reflective' },
          { title: 'Discernment', value: 'discernment' },
          { title: 'Readiness', value: 'readiness' },
          { title: 'Concern Check', value: 'concernCheck' }
        ]
      },
      initialValue: 'discernment',
      validation: Rule => Rule.required()
    }),

    // -------------------------
    // Context
    // -------------------------

    defineField({
    name: 'placements',
    title: 'Placements',
    description:
      'Controls where this prompt appears and in what order. Add one placement for each milestone and dimension where this prompt should be used.',
    type: 'array',
    fieldset: 'context',
    of: [
      {
        type: 'object',
        name: 'promptPlacement',
        title: 'Prompt Placement',
        fields: [
          defineField({
            name: 'milestone',
            title: 'Milestone',
            type: 'reference',
            to: [{ type: 'milestone' }],
            validation: Rule => Rule.required()
          }),

          defineField({
            name: 'dimension',
            title: 'Dimension',
            type: 'reference',
            to: [{ type: 'dimension' }],
            validation: Rule => Rule.required()
          }),

          defineField({
            name: 'sortOrder',
            title: 'Sort Order',
            description:
              'Question order for this specific milestone and dimension.',
            type: 'number',
            validation: Rule => Rule.required().integer().min(1)
          })
        ],

        preview: {
          select: {
            milestone: 'milestone.title',
            dimension: 'dimension.title',
            sortOrder: 'sortOrder'
          },
          prepare(selection) {
            const { milestone, dimension, sortOrder } = selection

            return {
              title: [milestone, dimension].filter(Boolean).join(' • '),
              subtitle: sortOrder ? `Question ${sortOrder}` : 'No sort order'
            }
          }
        }
      }
    ],
    validation: Rule => Rule.required().min(1)
  }),

    // -------------------------
    // Prompt
    // -------------------------

    defineField({
      name: 'introduction',
      title: 'Introduction',
      type: 'text',
      rows: 3,
      fieldset: 'prompt'
    }),

    defineField({
      name: 'question',
      title: 'Question',
      type: 'text',
      rows: 3,
      fieldset: 'prompt',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'scriptureReference',
      title: 'Scripture Reference',
      type: 'string',
      fieldset: 'prompt'
    }),

    defineField({
      name: 'scriptureText',
      title: 'Scripture Text',
      type: 'text',
      rows: 4,
      fieldset: 'prompt'
    }),

    // -------------------------
    // Answer Choices
    // -------------------------

    defineField({
      name: 'answerType',
      title: 'Answer Type',
      type: 'string',
      fieldset: 'answers',
      options: {
        list: [
          { title: 'Single Choice', value: 'singleChoice' },
          { title: 'Yes / No', value: 'yesNo' }
        ]
      },
      initialValue: 'singleChoice',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'answerOptions',
      title: 'Answer Options',
      description: 'Structured choices used by the app to evaluate readiness or concern level.',
      type: 'array',
      fieldset: 'answers',
      of: [
        {
          type: 'object',
          name: 'answerOption',
          title: 'Answer Option',
          fields: [
            defineField({
              name: 'label',
              title: 'Label',
              description: 'The answer choice shown to the user.',
              type: 'string',
              validation: Rule => Rule.required()
            }),

            defineField({
              name: 'description',
              title: 'Description',
              description: 'Optional supporting explanation shown under the label.',
              type: 'text',
              rows: 2
            }),

            defineField({
              name: 'status',
              title: 'Internal Status',
              description: 'Used by the app for insight generation. This should not be shown immediately after the user answers.',
              type: 'string',
              options: {
                list: [
                  { title: 'Healthy', value: 'healthy' },
                  { title: 'Needs Attention', value: 'needsAttention' }
                ]
              },
              validation: Rule => Rule.required()
            }),

            defineField({
              name: 'concernLevel',
              title: 'Internal Concern Level',
              description: 'Used by the app for insight generation. This should not be shown immediately after the user answers.',
              type: 'string',
              options: {
                list: [
                  { title: 'None', value: 'none' },
                  { title: 'Low', value: 'low' },
                  { title: 'Moderate', value: 'moderate' },
                  { title: 'High', value: 'high' },
                  { title: 'Critical', value: 'critical' }
                ]
              },
              initialValue: 'none',
              validation: Rule => Rule.required()
            }),

            defineField({
              name: 'responseMessage',
              title: 'Immediate Response Message',
              type: 'text',
              rows: 2,
              description: 'Shown immediately after the user selects this answer.',
            }),
            
            defineField({
              name: 'insightMessage',
              title: 'Insight Message',
              description: 'Shown later on the Insight page under what looks steady or what may need attention. Do not include status labels here.',
              type: 'text',
              rows: 3
            }),

            defineField({
              name: 'sortOrder',
              title: 'Sort Order',
              type: 'number',
              validation: Rule => Rule.required().integer().min(1)
            }),

            defineField({
              name: 'nextPrompt',
              title: 'Next Prompt',
              description: 'Optional. If selected, this answer sends the user to a specific next prompt instead of the next prompt by sort order.',
              type: 'reference',
              to: [{ type: 'reflectionPrompt' }]
            }),

            defineField({
              name: 'endsPath',
              title: 'Ends This Reflection Path',
              description: 'Use this when this answer should stop the current dimension reflection path.',
              type: 'boolean',
              initialValue: false
            }),

            defineField({
              name: 'progressStatus',
              title: 'Progress Status',
              description: 'Optional status to save if this answer ends or redirects the path.',
              type: 'string',
              options: {
                list: [
                  { title: 'In Progress', value: 'in_progress' },
                  { title: 'Completed', value: 'completed' },
                  { title: 'Redirected', value: 'redirected' },
                  { title: 'Needs Guidance', value: 'needs_guidance' }
                ],
                layout: 'dropdown'
              }
            }),

            defineField({
              name: 'outcomeKey',
              title: 'Outcome Key',
              description: 'Internal key for the result of this answer, such as not_ready_for_courtship or needs_clarity.',
              type: 'string'
            }),

            defineField({
              name: 'outcomeMessage',
              title: 'Outcome Message',
              description: 'User-facing message shown when this answer ends or redirects the reflection path.',
              type: 'text',
              rows: 4
            }),

            defineField({
              name: 'recommendedMilestone',
              title: 'Recommended Milestone',
              description: 'Optional. Use this if this answer should guide the user toward another milestone.',
              type: 'reference',
              to: [{ type: 'milestone' }]
            }),

            defineField({
              name: 'actionSteps',
              title: 'Action Steps',
              type: 'array',
              of: [
                {
                  type: 'object',
                  name: 'actionStep',
                  title: 'Action Step',
                  fields: [
                    {
                      name: 'title',
                      title: 'Title',
                      type: 'string',
                      validation: (Rule) => Rule.required(),
                    },
                    {
                      name: 'description',
                      title: 'Description',
                      type: 'text',
                      rows: 3,
                    },
                    {
                      name: 'category',
                      title: 'Category',
                      type: 'string',
                      options: {
                        list: [
                          { title: 'Practical Step', value: 'practical_step' },
                          { title: 'Spiritual Practice', value: 'spiritual_practice' },
                          { title: 'Community Support', value: 'community_support' },
                          { title: 'Reflection', value: 'reflection' },
                          { title: 'Resource', value: 'resource' },
                        ],
                      },
                    },
                    {
                      name: 'sortOrder',
                      title: 'Sort Order',
                      type: 'number',
                      initialValue: 1,
                    },
                    {
                      name: 'resourceLabel',
                      title: 'Resource Label',
                      type: 'string',
                    },
                    {
                      name: 'resourceUrl',
                      title: 'Resource URL',
                      type: 'url',
                    },
                  ],
                  preview: {
                    select: {
                      title: 'title',
                      category: 'category',
                    },
                    prepare(selection) {
                      const { title, category } = selection

                      return {
                        title: title || 'Untitled action step',
                        subtitle: category ? `Category: ${category}` : 'Action step',
                      }
                    },
                  },
                },
              ],
            }),
          ],

          preview: {
            select: {
              title: 'label',
              subtitle: 'concernLevel'
            }
          }
        }
      ],
      validation: Rule => Rule.required().min(2)
    }),

    // -------------------------
    // Optional Notes
    // -------------------------

    defineField({
      name: 'allowNotes',
      title: 'Allow Notes',
      description: 'Allows the user to write an optional note about why they chose an answer.',
      type: 'boolean',
      initialValue: true,
      fieldset: 'notes'
    }),

    defineField({
      name: 'notesPrompt',
      title: 'Notes Prompt',
      description: 'Optional journaling prompt shown below the answer choices.',
      type: 'text',
      rows: 2,
      fieldset: 'notes',
      initialValue: 'Use this space to briefly note why you chose your answer.'
    }),

    // -------------------------
    // Guidance
    // -------------------------

    defineField({
      name: 'reflectionGuide',
      title: 'Reflection Guide',
      description: 'General guidance shown with the prompt.',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'warningNotes',
      title: 'Warning Notes',
      description: 'Important patterns the user should prayerfully notice.',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    // -------------------------
    // Publishing
    // -------------------------

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
      placements: 'placements'
    },
    prepare(selection) {
      const { title, placements } = selection

      const placementCount = Array.isArray(placements)
        ? placements.length
        : 0

      return {
        title,
        subtitle:
          placementCount === 1
            ? '1 placement'
            : `${placementCount} placements`
      }
    }
  }
})